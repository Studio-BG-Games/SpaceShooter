using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Base class for a linkable variable. This is a variable that is either set directly in the inspector, or,
    /// using a delegate encapsulating a getter/setter or other method, is linked to a value from another script.
    /// </summary>
    [System.Serializable]
    public class LinkableVariable
    {
        // Delay reordering of list when this value changes 
        [Delayed(order = 0)]
        public int listIndex = 0;
     
        // The key for this variable
        [SerializeField]
        protected string key;
        public string Key { get { return key; } }

        // The linkable variable type
        [SerializeField]
        protected LinkableVariableType variableType;
        public LinkableVariableType LinkableVariableType { get { return variableType; } }

        // Whether it's a linked variable (i.e. not a standalone variable but a link to a method from a script)
        [SerializeField]
        protected bool isLinkedVariable;

        // Input variable values
        [SerializeField]
        protected UnityEngine.Object objectValue;

        [SerializeField]
        protected bool boolValue;

        [SerializeField]
        protected int intValue;

        [SerializeField]
        protected float floatValue;

        [SerializeField]
        protected string stringValue;

        [SerializeField]
        protected Vector3 vector3Value;


        // The delegate that wraps the method that returns the value to the dictionary.

        [SerializeField]
        protected Func<UnityEngine.Object> objectMethodDelegate;

        [SerializeField]
        protected Func<bool> boolMethodDelegate;

        [SerializeField]
        protected Func<int> intMethodDelegate;

        [SerializeField]
        protected Func<float> floatMethodDelegate;

        [SerializeField]
        protected Func<string> stringMethodDelegate;

        [SerializeField]
        protected Func<Vector3> vector3MethodDelegate;


        // Info about the object the linked method is on

        [SerializeField]
        protected UnityEngine.Object targetObject;

        [SerializeField]
        protected UnityEngine.Object targetComponent;

        [SerializeField]
        protected string methodInfoName;

        [SerializeField]
        protected MethodInfo methodInfo;


        // Argument info for the linked method

        [SerializeField]
        protected int numArgs;

        [SerializeField]
        protected string arg0Type;

        [SerializeField]
        protected bool arg0BoolValue;

        [SerializeField]
        protected int arg0IntValue;

        [SerializeField]
        protected float arg0FloatValue;

        [SerializeField]
        protected string arg0StringValue;

        [SerializeField]
        protected UnityEngine.Object arg0ObjectValue;


        /// <summary>
        /// Initialize the linked variable.
        /// </summary>
        public void InitializeLinkDelegate()
        {

            if (!isLinkedVariable) return;

            // Get the currently selected method
            if (numArgs == 1)
            {
                methodInfo = targetComponent.GetType().GetMethod(methodInfoName, new Type[] { Type.GetType(arg0Type) });
            }
            else
            {
                methodInfo = targetComponent.GetType().GetMethod(methodInfoName, new Type[] { });
            }

            if (methodInfo == null)
            {
                Debug.LogError("Linkable variable with key " + key + " is set as a Linked Variable but has no function assigned. Assign a function to this linked variable in the inspector.");
                return;
            }

            // Get the type of the first argument
            System.Type argType = Type.GetType(arg0Type);

            // Get the argument object
            object thisArgObject = null;
            if (argType != null)
            {
                if (argType == typeof(bool))
                {
                    thisArgObject = arg0BoolValue;
                }
                else if (argType == typeof(int) || argType.IsEnum)
                {
                    if (argType.IsEnum)
                    {
                        thisArgObject = System.Enum.ToObject(argType, arg0IntValue);
                    }
                    else
                    {
                        thisArgObject = arg0IntValue;
                    }
                }
                else if (argType == typeof(float))
                {
                    thisArgObject = arg0FloatValue;
                }
                else if (argType == typeof(string))
                {
                    thisArgObject = arg0StringValue;
                }
                else
                {
                    thisArgObject = arg0ObjectValue == null ? null : arg0ObjectValue;
                }
            }
            
            // Create the delegate for accessing the method without garbage (i.e. without boxing the value)
            MethodInfo creatorMethod;
            if (methodInfo.ReturnType == typeof(UnityEngine.Object))
            {
                if (numArgs == 0)
                {
                    objectMethodDelegate = (Func<UnityEngine.Object>)Delegate.CreateDelegate(typeof(Func<UnityEngine.Object>), targetComponent, methodInfo);
                }
                else
                {
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(UnityEngine.Object));

                    objectMethodDelegate = (Func<UnityEngine.Object>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
            else if (methodInfo.ReturnType == typeof(bool))
            {
                if (numArgs == 0)
                {
                    boolMethodDelegate = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), targetComponent, methodInfo);
                }
                else
                {
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(bool));

                    boolMethodDelegate = (Func<bool>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
            else if (methodInfo.ReturnType == typeof(int))
            {
                if (numArgs == 0)
                {
                    intMethodDelegate = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), targetComponent, methodInfo);
                }
                else
                {
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(int));

                    intMethodDelegate = (Func<int>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
            else if (methodInfo.ReturnType == typeof(float))
            {
                if (numArgs == 0)
                {
                    floatMethodDelegate = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), targetComponent, methodInfo);
                }
                else
                {                   
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(float));
                    
                    floatMethodDelegate = (Func<float>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
            else if (methodInfo.ReturnType == typeof(string))
            {
                if (numArgs == 0)
                {
                    stringMethodDelegate = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), targetComponent, methodInfo);
                }
                else
                {
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(string));

                    stringMethodDelegate = (Func<string>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
            else if (methodInfo.ReturnType == typeof(Vector3))
            {
                if (numArgs == 0)
                {
                    vector3MethodDelegate = (Func<Vector3>)Delegate.CreateDelegate(typeof(Func<Vector3>), targetComponent, methodInfo);
                }
                else
                {
                    creatorMethod = typeof(LinkableVariable).GetMethod("GetDelegateWithArgumentContainer").MakeGenericMethod(argType, typeof(Vector3));
                    vector3MethodDelegate = (Func<Vector3>)creatorMethod.Invoke(this, new object[] { targetComponent, methodInfo, thisArgObject });
                }
            }
        }

        public UnityEngine.Object ObjectValue
        {
            get
            {
                if (isLinkedVariable)
                {
                    return objectMethodDelegate();
                }
                else
                {
                    return objectValue;
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    objectValue = value;
                }
            }
        }
       
        /// <summary>
        /// Get the bool value.
        /// </summary>
        /// <returns>The bool value.</returns>
        public bool BoolValue
        {
            get
            {
                if (isLinkedVariable)
                {
                    return boolMethodDelegate();
                }
                else
                {
                    return boolValue;
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    boolValue = value;
                }
            }
        }

        /// <summary>
        /// Get the int value.
        /// </summary>
        /// <returns>The int value.</returns>
        public int IntValue
        {
            get
            {
                if (isLinkedVariable)
                {
                    return intMethodDelegate();
                }
                else
                {
                    return intValue;
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    intValue = value;
                }
            }
        }

        /// <summary>
        /// Get the float value.
        /// </summary>
        /// <returns>The float value.</returns>
        public float FloatValue
        {
            get
            {
                if (isLinkedVariable)
                {
                    return floatMethodDelegate();
                }
                else
                {
                    return floatValue;
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    floatValue = value;
                }
            }
        }

        /// <summary>
        /// Get the Vector3 value.
        /// </summary>
        /// <returns>The Vector3 value.</returns>
        public Vector3 Vector3Value
        {
            get
            {
                if (isLinkedVariable)
                {
                    return vector3MethodDelegate();
                }
                else
                {
                    return vector3Value;
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    vector3Value = value;
                }
            }
        }

        /// <summary>
        /// Get the string value.
        /// </summary>
        /// <returns>The string value.</returns>
        public string StringValue
        {
            get
            {
                if (isLinkedVariable)
                {
                    // Convert to string
                    if (variableType == LinkableVariableType.String)
                    {
                        return stringMethodDelegate();
                    }
                    else if (variableType == LinkableVariableType.Bool)
                    {
                        return boolMethodDelegate().ToString();
                    }
                    else if (variableType == LinkableVariableType.Int)
                    {
                        return intMethodDelegate().ToString();
                    }
                    else if (variableType == LinkableVariableType.Float)
                    {
                        return floatMethodDelegate().ToString();
                    }
                    else if (variableType == LinkableVariableType.Vector3)
                    {
                        return (vector3MethodDelegate().ToString());
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    // Convert to string
                    if (variableType == LinkableVariableType.String)
                    {
                        return stringValue;
                    }
                    else if (variableType == LinkableVariableType.Bool)
                    {
                        return boolValue.ToString();
                    }
                    else if (variableType == LinkableVariableType.Int)
                    {
                        return intValue.ToString();
                    }
                    else if (variableType == LinkableVariableType.Float)
                    {
                        return floatValue.ToString();
                    }
                    else if (variableType == LinkableVariableType.Vector3)
                    {
                        return (vector3Value.ToString());
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            set
            {
                if (!isLinkedVariable)
                {
                    stringValue = value;
                }
            }
        }

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

        
        public void UsedOnlyForAOTCodeGeneration()
        {

            //GetDelegateWithArgumentContainer<HealthType, float>(null, null, HealthType.Armor);

            GetDelegateWithArgumentContainer<bool, bool>(null, null, true);
            GetDelegateWithArgumentContainer<int, bool>(null, null, 0);
            GetDelegateWithArgumentContainer<float, bool>(null, null, 0);
            GetDelegateWithArgumentContainer<string, bool>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, bool>(null, null, null);

            GetDelegateWithArgumentContainer<bool, int>(null, null, true);
            GetDelegateWithArgumentContainer<int, int>(null, null, 0);
            GetDelegateWithArgumentContainer<float, int>(null, null, 0);
            GetDelegateWithArgumentContainer<string, int>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, int>(null, null, null);

            GetDelegateWithArgumentContainer<bool, float>(null, null, true);
            GetDelegateWithArgumentContainer<int, float>(null, null, 0);
            GetDelegateWithArgumentContainer<float, float>(null, null, 0);
            GetDelegateWithArgumentContainer<string, float>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, float>(null, null, null);

            GetDelegateWithArgumentContainer<bool, string>(null, null, true);
            GetDelegateWithArgumentContainer<int, string>(null, null, 0);
            GetDelegateWithArgumentContainer<float, string>(null, null, 0);
            GetDelegateWithArgumentContainer<string, string>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, string>(null, null, null);

            GetDelegateWithArgumentContainer<bool, UnityEngine.Object>(null, null, true);
            GetDelegateWithArgumentContainer<int, UnityEngine.Object>(null, null, 0);
            GetDelegateWithArgumentContainer<float, UnityEngine.Object>(null, null, 0);
            GetDelegateWithArgumentContainer<string, UnityEngine.Object>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, UnityEngine.Object>(null, null, null);

            GetDelegateWithArgumentContainer<bool, Vector3>(null, null, true);
            GetDelegateWithArgumentContainer<int, Vector3>(null, null, 0);
            GetDelegateWithArgumentContainer<float, Vector3>(null, null, 0);
            GetDelegateWithArgumentContainer<string, Vector3>(null, null, "");
            GetDelegateWithArgumentContainer<UnityEngine.Object, Vector3>(null, null, null);


            // Include an exception so we can be sure to know if this method is ever called.
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
        
    }
}