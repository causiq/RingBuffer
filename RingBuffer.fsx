#r "packages/Hopac/lib/net45/Hopac.Core.dll"
#r "packages/Hopac/lib/net45/Hopac.Platform.dll"
#r "packages/Hopac/lib/net45/Hopac.dll"
#r "packages/Mono.Cecil/lib/net45/Mono.Cecil.Rocks.dll"
#r "packages/Mono.Cecil/lib/net45/Mono.Cecil.dll"
#r "packages/Expecto/lib/net40/Expecto.dll"
#load "RingBuffer.fs"
open Hopac
open Expecto

let tests =
  testList "RingBuffer" [
    testCaseAsync "enqueue" (job {
      let! rb = RingBuffer.create 2u
      do! RingBuffer.put rb "a"
      let! res = RingBuffer.take rb
      Expect.equal res "a" "Got value"
    } |> Job.toAsync)
  ]

Tests.runTests defaultConfig tests