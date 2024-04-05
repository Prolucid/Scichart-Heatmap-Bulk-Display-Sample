open Elmish
open Elmish.WPF
//open FSharp.Core
open SciChart.Charting.Model.DataSeries.Heatmap2DArrayDataSeries
open BulkLoadViews
open System

type Model = {
    GraphData: UniformHeatmapDataSeries<double,double,int16> array
    DisplayIndex: int
    IsNextFrameLoaded: bool
}

type Msg =
    | ShowNextFrame
    | NewGraphLoaded of UniformHeatmapDataSeries<double, double, int16>

// Async function to generate a new frame of randomized data
let generateNewGraphData dispatch =
    let buildGraph onDone =
        async {
            let randomGenerator = Random()
            UniformHeatmapDataSeries<double,double,int16>(Array2D.init 10000 10000 (fun i j -> randomGenerator.Next(101) |> int16), 0, 1, 0, 1) |> onDone
        }
    async {
        let onDone = 
            fun newGraph -> newGraph |> NewGraphLoaded |> dispatch
        do! buildGraph onDone
    } |> Async.Start

let init(): Model*Cmd<Msg> = 
    let randomGenerator = Random()
    {
        GraphData = [| 
            UniformHeatmapDataSeries<double,double,int16>(Array2D.init 10000 10000 (fun i j -> randomGenerator.Next(101) |> int16), 0,1,0,1)
            UniformHeatmapDataSeries<double,double,int16>(Array2D.init 10000 10000 (fun i j -> randomGenerator.Next(101) |> int16), 0,1,0,1)
        |]
        DisplayIndex = 0
        IsNextFrameLoaded = true
    }, Cmd.none

let update (msg: Msg) (model: Model) : Model*Cmd<Msg> =
    match msg with
    // Display the next frame in the graph data
    | ShowNextFrame ->
        model.GraphData[model.DisplayIndex].Clear()
        { model with DisplayIndex = (model.DisplayIndex + 1) % model.GraphData.Length 
                     IsNextFrameLoaded = false },
        generateNewGraphData |> Cmd.ofSub

    // Callback when new graph data is generated
    | NewGraphLoaded newGraph ->
        { model with GraphData = model.GraphData |> Array.mapi (fun i x -> if i = (model.DisplayIndex + 1) % model.GraphData.Length then newGraph else x)
                     IsNextFrameLoaded = true },
        Cmd.none

let bindings (): Binding<Model, Msg> list = [
    "GraphData" |> Binding.oneWay (fun m -> m.GraphData[m.DisplayIndex])
    "ShowNextFrame" |> Binding.cmd ShowNextFrame
    "IsNextFrameLoaded" |> Binding.oneWay (fun m -> m.IsNextFrameLoaded)
    "CurrentFrame" |> Binding.oneWay (fun m -> m.DisplayIndex)
]

[<EntryPoint; STAThread>]
let main _ =
  Program.mkProgramWpf init update bindings
  |> Program.runWindow (Window1())