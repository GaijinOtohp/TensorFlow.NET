﻿using System;
using Tensorflow.Functions;
using Tensorflow.Train;

namespace Tensorflow
{
    public class Function: Trackable
    {
#pragma warning disable CS0169 // The field 'Function._handle' is never used
        private IntPtr _handle;
#pragma warning restore CS0169 // The field 'Function._handle' is never used

        protected Func<Tensor[], Tensor[]> _csharp_function;
        protected ConcreteFunction _concrete_variable_creation_fn;
        protected bool _autograph;
        protected TracingCompiler _variable_creation_fn;
        protected bool _has_initialized;
        public string Name { get; set; }
        public Function(Func<Tensor[], Tensor[]> csharp_function, 
            string name, bool auto_graph = true)
        {
            _csharp_function = csharp_function;
            Name = name;
            _autograph = auto_graph;
            _has_initialized = false;
        }

        public virtual Tensors Apply(Tensors inputs)
        {
            if (_run_functions_eagerly())
            {
                return _csharp_function(inputs);
            }

            var result = _call(inputs);
            return result;
        }

        protected virtual Tensors _call(Tensors inputs)
        {
            if (!_has_initialized)
            {
                _initialize(inputs);
            }

            return _concrete_variable_creation_fn.CallFlat(inputs,
                _concrete_variable_creation_fn.CapturedInputs);
        }

        protected TracingCompiler _compiler(Func<Tensor[], Tensor[]> fn)
        {
            var name = nameof(fn);
            return new TracingCompiler(fn, name, autograph: _autograph);
        }

        protected virtual bool _run_functions_eagerly()
        {
            return false;
        }

        private void _initialize(Tensor[] args)
        {
            _variable_creation_fn = _compiler(_csharp_function);
            _variable_creation_fn._name = this.Name;
            _concrete_variable_creation_fn = _variable_creation_fn._get_concrete_function_internal_garbage_collected(args);
            _has_initialized = true;
        }
    }
}
