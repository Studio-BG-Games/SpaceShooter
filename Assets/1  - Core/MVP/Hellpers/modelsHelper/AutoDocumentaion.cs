using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace ModelCore
{
    public static class AutoDocumentaion
    {
        public static string DocumentationTypes(Type[] types)
        {
            string result = "";
            types.OrderBy(CompersionType);
            foreach (var type in types)
            {
                if (type.IsEnum) result += $"{DocumentEnum(type)}\n";
                else if (type.IsInterface) result += $"{DocumentInterface(type)}\n";
                else if (type.IsClass) result += $"{DocumentClass(type)}\n";
                else result += $"Не смог распознать - {type.Name}\n";
            }

            return result;
        }

        private static int CompersionType(Type type)
        {
            if (type.IsEnum) return 3;
            else if (type.IsInterface) return 2;
            else if (type.IsClass) return 1;
            else return 4;
        }


        private static string DocumentEnum(Type type)
        {
            string name = type.Name;
            var namesValue = type.GetEnumNames();
            var values = "";
            for (int i = 0; i < namesValue.Length; i++)
            {
                values += $"{namesValue[i]}, ";
                if (i % 5 == 0)
                    values += "\n";
            }

            return $"Enum - {name}\n" +
                   $"    Values:\n{values}";
        }
        
        private static string DocumentInterface(Type type)
        {
            string name = type.Name;
            string intrefaces = $"Интерфейсы - {TypeToString(type.GetInterfaces())}";
            
            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
            var methods = type.GetMethods(flags);
            var property = type.GetProperties(flags);
            
            string methodsDoc = DocumentMethod(methods);
            string propertyDoc = DocumentProp(property);

            return $"Интерфейс {name}\n" +
                   $"    Интерфейсы: {intrefaces}\n" +
                   $"    Методы:\n{methodsDoc}\n" +
                   $"    Параметры:\n{propertyDoc}\n";
        }
        
        private static string DocumentClass(Type type)
        {
            string name = type.Name;
            string inhert = $"Наследование от - {type.BaseType.Name}";
            string intrefaces = $"Интерфейсы - {TypeToString(type.GetInterfaces())}";
            string isAbs = type.IsAbstract ? "Абстрактный " : "";

            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;

            var methods = type.GetMethods(flags);
            var property = type.GetProperties(flags);
            var fields = type.GetFields(flags);

            string methodsDoc = DocumentMethod(methods);
            string propertyDoc = DocumentProp(property);
            string fieldsDoc = DocumentField(fields);

            return $"{isAbs}Класс {name} от {inhert}\n" +
                   $"    Интерфейсы: {intrefaces}\n" +
                   $"    Методы:\n{methodsDoc}\n" +
                   $"    Параметры:\n{propertyDoc}\n" +
                   $"    Поля:\n{fieldsDoc}";
        }

        private static string DocumentField(FieldInfo[] fields)
        {
            string result = "";
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var name = field.Name;
                var type = field.FieldType;
                result += $"        {name} : {type}\n";
            }

            return result;
        }

        private static string DocumentProp(PropertyInfo[] property)
        {
            string result = "";
            for (int i = 0; i < property.Length; i++)
            {
                var prop = property[i];
                
                var dostupRead ="";
                if(prop.CanRead)
                    dostupRead =  prop.GetMethod.IsPublic ? "Public " : "Private or protected ";

                string dostupWrite = "";
                if(prop.CanWrite)
                    dostupWrite =prop.SetMethod.IsPublic ? "Public " : "Private or protected ";
                
                var canRead = dostupRead + (prop.CanRead ? "Читаемо" : "Не читаемо");
                var canWrite = dostupWrite + (prop.CanWrite ? "Записаемое" : "Не записаемое");
                result += $"        {prop.Name} : {prop.PropertyType.Name} ({canRead}, {canWrite})\n";
            }

            return result;
        }

        private static string DocumentMethod(MethodInfo[] methods)
        {
            string result = "";
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var returnedType = method.ReturnType.Name;
                var name = method.Name;
                string requreType = "";
                method.GetParameters().ForEach(x => requreType += $"{x.ParameterType}, ");

                result += $"        {name}({requreType}) : {returnedType}\n";
            }
            return result;
        }

        private static string TypeToString(Type[] types)
        {
            string result = "";
            types.ForEach(x => result += $"{x.Name}, ");
            return result;
        }
    }
}