#r "packages/Hopac/lib/net45/Hopac.Core.dll"
#r "packages/Hopac/lib/net45/Hopac.Platform.dll"
#r "packages/Hopac/lib/net45/Hopac.dll"
#r "packages/Mono.Cecil/lib/net45/Mono.Cecil.Rocks.dll"
#r "packages/Mono.Cecil/lib/net45/Mono.Cecil.dll"
#r "packages/Expecto/lib/net40/Expecto.dll"
#load "RingBuffer.fs"
open Hopac
open Expecto
open Hopac.Infixes

let tests =
  testList "RingBuffer" [
    testCaseAsync "enqueue" (job {
      let! rb = RingBuffer.create 4us

      let mutable result = []
      
      let takePerSecond = Job.delay <| fun () ->
        timeOutMillis 100 ^=>. job {
          let! res = RingBuffer.take rb
          result <- ( res :: result)
        }
      Job.foreverServer takePerSecond |> run
      
      do! RingBuffer.put rb "a"
      do! RingBuffer.put rb "b"
      do! RingBuffer.put rb "c"
      do! RingBuffer.put rb "d"
      do! RingBuffer.put rb "e"
      do! RingBuffer.put rb "f"
      do! RingBuffer.put rb "g"

      do! timeOutMillis 1000 ^=>. job {
            Expect.equal result (List.rev ["a";"b";"c";"d";"e";"f";"g"]) "Got value"
          }
    } |> Job.toAsync)

    testCaseAsync "take all/batch" (job {
      let! rb = RingBuffer.create 4us
      
      do! RingBuffer.put rb "a"
      do! RingBuffer.put rb "b"
      do! RingBuffer.put rb "c"
      do! RingBuffer.put rb "d"
      let! res = RingBuffer.takeBatch 3us rb
      Expect.equal res ([|"a";"b";"c"|]) "Got value"
      do! RingBuffer.put rb "e"
      do! RingBuffer.put rb "f"
      do! RingBuffer.put rb "g"

      let! res = RingBuffer.takeAll rb
      Expect.equal res ([|"d";"e";"f";"g";|]) "Got value"

    } |> Job.toAsync)

    testList "ringSizeValidate" [
      test "0 is not allowed" {
        Expect.equal (Utils.ringSizeValidate 0us) false "(ringSizeValidate 0)"
      }

      test "1 is allowed" {
        Expect.equal (Utils.ringSizeValidate 1us) true "(ringSizeValidate 1)"
      }

      test "is power of 2" {
        Expect.equal (Utils.ringSizeValidate 8us) true "(ringSizeValidate 8)"
      }

      test "half the range of the index data types (uint16)" {
        Expect.equal (Utils.ringSizeValidate (uint16 (System.Math.Pow(2.,15.)))) true "(ringSizeValidate pow 2 15)"
      }
    ] 
    
  ]


Tests.runTests defaultConfig tests
