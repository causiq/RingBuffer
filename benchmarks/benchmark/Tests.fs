module Tests

open Expecto
open BenchmarkDotNet.Attributes.Exporters
open Hopac
open Hopac.Infixes
open BenchmarkDotNet.Attributes


[<MarkdownExporterAttribute.GitHub>]
type Ring() =
  let mutable ringOld:Old.RingBuffer<int> option = None
  let mutable ringNew:RingBuffer<int> option = None

  [<GlobalSetup>]
  member __.setup() =
    let setupRingOldJob = job {
      let! ring = Old.RingBuffer.create 2us
      do! Old.RingBuffer.put ring 1
      do! Old.RingBuffer.put ring 2
      return ring
    }

    let setupRingNewJob = job {
      let! ring = RingBuffer.create 2us
      do! RingBuffer.put ring 1
      do! RingBuffer.put ring 2
      return ring
    }

    ringOld <- setupRingOldJob |> Hopac.run |> Some
    ringNew <- setupRingNewJob |> Hopac.run |> Some


  [<Benchmark>]
  member __.tryPutChannel() =
    let ring = ringOld.Value
    let rec loop times =
      match times with
      | 0 -> Job.result ()
      | _ ->
        (Old.RingBuffer.tryPut ring times ^=> fun _ -> loop (times - 1)) :> Job<_>

    loop 1000000 |> Hopac.run


  [<Benchmark>]
  member __.tryPutMVar() =
    let ring = ringNew.Value
    let rec loop times =
      match times with
      | 0 -> Job.result ()
      | _ ->
        (RingBuffer.tryPut ring times ^=> fun _ -> loop (times - 1)) :> Job<_>

    loop 1000000 |> Hopac.run


[<Tests>]
let tests =
  testSequenced <| testList "benmark" [
    test "try put benchmark" {
      benchmark<Ring> benchmarkConfig (fun a -> null) |> ignore
    }
  ]