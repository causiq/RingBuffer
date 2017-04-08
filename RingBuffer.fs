module Hopac

open Hopac
open Hopac.Infixes

type RingBuffer<'a> =
  private {putCh : Ch<'a>; takeCh : Ch<'a>; takeBatchCh : Ch<int*IVar<'a[]>>}

module RingBuffer =
  let create (ringSize : uint32) : Job<RingBuffer<'a>> =
    let ringSize = int ringSize
    let self = { putCh = Ch (); takeCh = Ch (); takeBatchCh = Ch () }
    let ring = Array.zeroCreate ringSize
    let limit = ringSize - 1
    let inline modulo x = x % ringSize
    let byteSize = sizeof<'a>
    let mutable head, tail = 0, 0
    let inline enqueue x =
      ring.[head] <- x
      head <- modulo <| head + 1
    let inline dequeue () =
      tail <- modulo <| tail + 1
    let inline count () =
      modulo <| ringSize + head - tail
    let dequeueBatch (max, ivar) =
      let dequeueCount = min max <| count ()
      let arr = Array.zeroCreate dequeueCount
      let stopIndex = tail + dequeueCount
      ivar *<=
        ( if stopIndex < ringSize then
            Array.blit ring tail arr 0 dequeueCount
            tail <- stopIndex
            arr
          else
            let stopIndex = modulo stopIndex
            Array.blit ring tail arr 0 (ringSize - tail)
            Array.blit ring 0 arr (ringSize - tail) stopIndex
            tail <- stopIndex
            arr )
    let put () = self.putCh ^-> enqueue
    let take () = self.takeCh *<- ring.[tail] ^-> dequeue
    let takeBatch () = self.takeBatchCh ^=> Job.delayWith dequeueBatch
    let proc = Job.delay <| fun () ->
      match count () with
      | 0 -> put ()
      | x when x = limit -> takeBatch () <|> take ()
      | _ -> takeBatch () <|> take () <|> put ()
    Job.foreverServer proc >>-. self
  let put q x = q.putCh *<- x
  let take q = q.takeCh :> Alt<_>
  let takeBatch (maxBatchSize : uint32) q = q.takeBatchCh *<-=>- (fun iv -> int maxBatchSize, iv)
  let takeAll q = takeBatch System.UInt32.MaxValue q

  let consume q s = Stream.iterJob (fun x -> q.putCh *<- x) s |> Job.start
  let tap q = Stream.indefinitely <| q.takeCh
  let tapBatches maxBatchSize q = Stream.indefinitely <| takeBatch maxBatchSize q
  let tapAll q = Stream.indefinitely <| takeAll q