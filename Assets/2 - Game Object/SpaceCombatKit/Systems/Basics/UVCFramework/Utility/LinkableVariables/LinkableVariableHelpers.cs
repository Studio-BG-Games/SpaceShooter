using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace VSX.UniversalVehicleCombat
{
    /*
    /// <summary>
    /// Creates a delegate container for a getter/setter or method obtained via reflection, so it can be called without garbage.
    /// </summary>
    public class DelegateWithArgumentContainerCreator
    {

        /// <summary>
        /// Create a delegate from a getter/setter or method, insert a predetermined argument, and return it as a simple Func with no argument 
        /// required.
        /// </summary>
        /// <typeparam name="TArg">The argument value to be used every time this method is called.</typeparam>
        /// <typeparam name="TValue">The return type of the delegate.</typeparam>
        /// <param name="obj">The object on which the method to encapsulated by the delegate exists.</param>
        /// <param name="methodInfo">The method to be encapsulated by the delegate.</param>
        /// <param name="arg">The argument to be inserted every time the delegate is called.</param>
        /// <returns> A zero-argument delegate.</returns>
        public Func<TValue> GetDelegateWithArgumentContainer<TArg, TValue>(object obj, MethodInfo methodInfo, TArg arg)
        {
            
            // Create the delegate
            Func<TArg, TValue> f = (Func<TArg, TValue>)Delegate.CreateDelegate(typeof(Func<TArg, TValue>), obj, methodInfo);

            // Create the container to hold both the delegate and the argument for it.
            DelegateWithArgumentContainer<TArg, TValue> variableContainer = new DelegateWithArgumentContainer<TArg, TValue>();
            variableContainer.arg = arg;
            variableContainer.function = f;

            // Return a delegate that wraps the initial delegate and its argument
            Func<TValue> result = variableContainer.GetValue;
            return result;
        }
    }

    */
    /// <summary>
    /// A container for holding a delegate with an argument value.
    /// </summary>
    /// <typeparam name="TArg">The argument type.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    public class DelegateWithArgumentContainer<TArg, TResult>
    {
        public Func<TArg, TResult> function;
        public TArg arg;

        /// <summary>
        /// Get the value of the delegate with the argument inserted.
        /// </summary>
        /// <returns>The value returned by the delegate with the argument inserted.</returns>
        public TResult GetValue()
        {
            return function(arg);
        }
    }
}