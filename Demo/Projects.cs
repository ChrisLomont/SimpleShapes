using System.Reflection;

namespace Lomont.Projects
{
    class Projects
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Starting simple 2D projects");
            
            // you can call your project directly, or use this to scan existing ones
            ScanAllProjects(args);

            Console.WriteLine("Finished!");
        }

        static void ScanAllProjects(string [] args)
        {
            // reflect on assembly to find all valid types
            var items = new List<(Type type,MethodInfo methodInfo, ProjectAttribute attr)>();
            Console.WriteLine("Parsing...");
            foreach (var type1 in GetTypesWithAttribute<ProjectAttribute>())
            {

                var methodInfo1 = type1.GetMethod("Run");
                if (!NoParameters(methodInfo1))
                {
                    Console.WriteLine($"Error: type {type1.Name} has missing or incorrect entry point.");
                }
                else
                {
                    var attr = (ProjectAttribute)type1.GetCustomAttributes(typeof(ProjectAttribute), true)[0];
                    items.Add((type1,methodInfo1,attr));
                }
            }

            // sort types so most recent at top
            items.Sort((a, b) => -a.attr.Date.CompareTo(b.attr.Date));

            // max lengths of text values
            var maxClassLen = items.Max(n => n.type.Name.Length);
            var maxDescLen   = items.Max(n => n.attr.Description.Length);
            var maxAuthorLen = items.Max(n => n.attr.Author.Length);

            // print types and info            
            var formatString = $"{{0,2}} : {{1,{maxClassLen}}}, {{2,{maxDescLen}}}, {{3:d}}, {{4,{maxAuthorLen}}}";

            Console.WriteLine("Projects: ------------------");
            for (var i = 0; i < items.Count; ++i)
            {
                var (type1, methodInfo1, attr) = items[i];
                Console.WriteLine(String.Format(formatString, i + 1, type1.Name, attr.Description, attr.Date, attr.Author));
            }

            Console.WriteLine("-------------------------------");
            Console.WriteLine($"Enter value 1-{items.Count}");
            var index = -1;
            if (args.Length >= 1 && int.TryParse(args[0], out var tempIndex1))
            {
                index = tempIndex1;
                Console.WriteLine(args[0]);
            }
            else if (int.TryParse(Console.ReadLine(), out var tempIndex2))
            {
                index = tempIndex2;
            }
            if (index >= 1)
            {
                var (type, methodInfo, attr) = items[index-1];

                // list and run?!
                Console.WriteLine($"Executing {type.Name}");

                // make instance and run it
                var instance = Activator.CreateInstance(type);
                methodInfo.Invoke(instance, null);
            }
            else
            {
                // todo - make work again- Console.WriteLine($"Error: {input} invalid input");
            }

        }

        // return true if type of method is void func(void)
        static bool NoParameters(MethodInfo methodInfo)
        {
            if (methodInfo == null) return false;
            var pi = methodInfo.GetParameters();
            return methodInfo.ReturnType == typeof(void) && pi != null && pi.Length == 0;
        }

        /// <summary>
        /// Enumerate types
        /// </summary>
        /// <typeparam name="AttributeType"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static IEnumerable<Type> GetTypesWithAttribute<AttributeType>(Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(AttributeType), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}
