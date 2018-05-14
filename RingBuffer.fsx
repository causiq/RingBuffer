#I "bin/Debug/net461"
#r "Hopac.Core"
#r "Hopac.Platform"
#r "Hopac"
#r "packages/Expecto/lib/net461/Expecto.dll"
#r "packages/Expecto.FsCheck/lib/net461/Expecto.dll"
#r "packages/Argu/lib/net40/Argu.dll"
#load "RingBuffer.fs"
open Hopac
open Expecto
open Hopac.Infixes
open Hopac.Extensions

let tests =
  testList "RingBuffer" [
    testCaseAsync "enqueue/dequeue" (job {
      let! rb = RingBuffer.create 4us
      let dataSource = ["a";"b";"c";"d";"e";"f";"g"]
      let latch = Latch dataSource.Length

      let mutable result = []

      let take =
        (RingBuffer.take rb ^=> fun res ->
           result <- (res :: result)
           Latch.decrement latch
           ) :> Job<_>

      do! Job.foreverServer take
      do! Seq.iterJobIgnore (fun v -> RingBuffer.put rb v) (List.rev dataSource)

      do! Latch.await latch

      Expect.equal result dataSource "Got value"

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

      test "is not power of 2" {
        Expect.equal (Utils.ringSizeValidate 7us) false "(ringSizeValidate 7)"
      }

      test "is power of 2" {
        Expect.equal (Utils.ringSizeValidate 8us) true "(ringSizeValidate 8)"
      }

      test "half the range of the index data types (uint16)" {
        Expect.equal (Utils.ringSizeValidate (uint16 (System.Math.Pow(2.,15.)))) true "(ringSizeValidate pow 2 15)"
      }
    ]

  ]

Tests.runTestsWithArgs defaultConfig [| "--summary"; "--debug" |] tests
