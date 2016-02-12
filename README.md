# RingBuffer

This repository contains an implementation of a ring buffer (also known as a
[circular buffer][WikiCircBuffer] or circular queue) written for use with the
[Hopac][] concurrency library. The ring buffer provided here is similar to the
[`BoundedMb<'a>`][HopacBMb] provided by Hopac, except that this implementation
uses a circular array instead of a `Queue<T>` as provided by the Base Class
Libraries. When used in the context of the Hopac libraries, this ring buffer
is thread-safe.

## Using `RingBuffer.fs` in your own project

This file can be added directly into your project or, if you are using [Paket][] to
manage your dependencies, it can be incorporated as a github dependency by adding the
following line to your `paket.dependencies` file:

    github logary/RingBuffer RingBuffer.fs

## About the ring buffer

Like a `BoundedMb<'a>`, the `RingBuffer<'a>` provides back-pressure to the upstream
producer of items in the event that the buffer is full. The `RingBuffer<'a>`
additionally provides the ability to dequeue batches of items as part of a single
`takeBatch` or `takeAll` operation. The `takeBatch` operation allows the caller to
specify a maximum batch size that they will accept.

This implementation also provides utility functions to produce and consume Hopac-style
streams.

As currently implemented, the `RingBuffer<'a>` is take-biased. This means that if
the buffer is partially filled and a producer is ready to put to the buffer at the
same time that a consumer is ready to take from the buffer, the take operation is
preferred. This precedence can be altered by modifying the order in which
`takeBatch ()`, `take ()` and `put ()` appear in the final match expression of the
`create` function. Fairness can be guaranteed at the expense of some additional
overhead by using the Hopac [`chooser`][HopacChooser] function. Do not just
change `<|>` to `<~>` as the latter [does not have the expected effect][Hopac<~>Warn]
in chains of three or more alternatives.

  [Hopac]: https://hopac.github.io/Hopac/Hopac.html
  [HopacBMb]: https://hopac.github.io/Hopac/Hopac.html#def:type%20Hopac.BoundedMb
  [HopacChooser]: https://hopac.github.io/Hopac/Hopac.html#def:val%20Hopac.Alt.chooser
  [Hopac<~>Warn]: https://hopac.github.io/Hopac/Hopac.html#def:val%20Hopac.Infixes.<~>
  [Paket]: https://fsprojects.github.io/Paket/github-dependencies.html
  [WikiCircBuffer]: http://en.wikipedia.org/wiki/Circular_buffer
  
### Possible future improvements

* Separate the notion of a buffer that produces single items from one that can produce
  batches.
* Allow for a hybrid precedence mode for batching, such that in the event of
  put/take-contention, puts will be preferred while the number of buffered events is
  below a certain limit. When the number of buffered events exceeds that limit, then
  takes will be preferred. This would ensure that batches are as large as
  possible while also freeing up space for additional items to be added to the
  buffer.
* Make a minor change to allow `ringSize` to be the actual capacity instead of
  `ringSize-1` as currently implemented. All that needs to be done here is have a
  boolean that tracks whether or not the last operation was a put or a take. Given
  an operation that results in `tail`'s index becoming equal to `head`'s index,
  if the last operation was a put, then the buffer is now full. If the last
  operation was a take, then the buffer is now empty.
* Streams produced by `tap`, `tapBatches`, and `tapAll` compete with each other to
  consume items from the ring buffer. In the case of a ring buffer, there is
  generally only one consumer of the stream. When there is more than one consumer,
  the competing model is likely the intended one. Other use cases may prefer that
  tapped streams are independent from each other such that each consumer sees the
  same set of generated batches from the point at which they tapped the buffer. 

## Maintainers

* [Marcus Griep](https://neoeinstein.github.io/) — [@neoeinstein](https://twitter.com/neoeinstein)
