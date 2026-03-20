using System;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Motwane.UVSS.Application.ComputerVision
{
    // Minimal DenseTensor<T> — OnnxRuntime 1.17+ removed it from the package,
    // so we provide our own concrete implementation of the abstract Tensor<T>.
    internal sealed class DenseTensor<T> : Tensor<T>
    {
        private readonly T[] _data;

        public DenseTensor(int[] dimensions) : base((ReadOnlySpan<int>)dimensions, false)
        {
            _data = new T[(int)Length];
        }

        public override T GetValue(int index) => _data[index];

        public override void SetValue(int index, T value) => _data[index] = value;

        public override Tensor<T> Clone()
        {
            int[] dims = new int[Dimensions.Length];
            for (int i = 0; i < dims.Length; i++) dims[i] = Dimensions[i];
            var clone = new DenseTensor<T>(dims);
            Array.Copy(_data, clone._data, _data.Length);
            return clone;
        }

        public override Tensor<T> Reshape(ReadOnlySpan<int> dimensions)
        {
            int[] newDims = dimensions.ToArray();
            var result = new DenseTensor<T>(newDims);
            Array.Copy(_data, result._data, Math.Min(_data.Length, result._data.Length));
            return result;
        }

        public override Tensor<TResult> CloneEmpty<TResult>(ReadOnlySpan<int> dimensions)
        {
            return new DenseTensor<TResult>(dimensions.ToArray());
        }
    }
}
