using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace Notes.Application.Common.Mappings
{
    public class AssemblyMappingProfile : Profile
    {
        public AssemblyMappingProfile(Assembly assembly) =>
            ApplyMappingsFromAssembly(assembly);

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IMapWith<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod("Mapping");

                if(methodInfo == null)
                {
                    var mapInterface = type.GetInterfaces()
                        .FirstOrDefault(w => w.Name.Remove(w.Name.IndexOf('`')) == "IMapWith");

                    var genericType = mapInterface.GetGenericArguments()[0];

                    var mapType = typeof(IMapWith<>).MakeGenericType(genericType);
                    methodInfo = mapType.GetMethod("Mapping");
                }

                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
