using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Nameless.EventSourcing {

    internal class CompiledMethodInfo {

        #region Private Delegates

        private delegate object ReturnValueDelegate(object instance, object[] arguments);

        private delegate void VoidDelegate(object instance, object[] arguments);

        #endregion

        #region Private Properties

        private ReturnValueDelegate Delegate { get; }

        #endregion

        #region Internal Constructors

        internal CompiledMethodInfo(MethodInfo methodInfo, Type type) {
            var instanceExpression = Expression.Parameter(typeof(object), "instance");
            var argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
            var parameterInfos = methodInfo.GetParameters();

            var argumentExpressions = new Expression[parameterInfos.Length];
            for (var idx = 0; idx < parameterInfos.Length; ++idx) {
                argumentExpressions[idx] = Expression.Convert(Expression.ArrayIndex(argumentsExpression, Expression.Constant(idx)), parameterInfos[idx].ParameterType);
            }
            var callExpression = Expression.Call(!methodInfo.IsStatic ? Expression.Convert(instanceExpression, type) : null, methodInfo, argumentExpressions);
            if (callExpression.Type == typeof(void)) {
                var voidDelegate = Expression.Lambda<VoidDelegate>(callExpression, instanceExpression, argumentsExpression).Compile();
                Delegate = (instance, arguments) => { voidDelegate(instance, arguments); return 0; };
            } else {
                Delegate = Expression.Lambda<ReturnValueDelegate>(Expression.Convert(callExpression, typeof(object)), instanceExpression, argumentsExpression).Compile();
            }
        }

        #endregion

        #region Internal Methods

        internal object Invoke(object instance, params object[] arguments) {
            return Delegate(instance, arguments);
        }

        #endregion
    }
}
