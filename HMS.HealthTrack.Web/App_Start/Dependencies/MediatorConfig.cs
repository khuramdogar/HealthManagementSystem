using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Practices.Unity;

namespace HMS.HealthTrack.Web.Dependencies
{
   internal static class MediatorConfig
   {
      internal static void ConfigMediator(this IUnityContainer container)
      {
         container.RegisterMediator(new HierarchicalLifetimeManager());
         container.RegisterMediatorHandlers("HMS.");
      }
   }

   public static class IUnityContainerExtensions
   {
      internal static IUnityContainer RegisterMediator(this IUnityContainer container, LifetimeManager lifetimeManager)
      {
         return container.RegisterType<IMediator, Mediator>(lifetimeManager)
             .RegisterInstance<ServiceFactory>(type =>
             {
                var enumerableType = type
                       .GetInterfaces()
                       .Concat(new[] { type })
                       .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return enumerableType != null
                       ? container.ResolveAll(enumerableType.GetGenericArguments()[0])
                       : container.IsRegistered(type)
                           ? container.Resolve(type)
                           : null;
             });
      }

      public static void RegisterMediatorHandlers(this IUnityContainer container, string assemblyFilter)
      {
         foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith(assemblyFilter)))
         {
            container.RegisterTypesImplementingType(assembly, typeof(IRequestHandler<,>))
               .RegisterNamedTypesImplementingType(assembly, typeof(INotificationHandler<>));
         }
      }

      internal static bool IsGenericTypeOf(this Type type, Type genericType)
      {
         return type.IsGenericType &&
                type.GetGenericTypeDefinition() == genericType;
      }

      internal static void AddGenericTypes(this List<object> list, IUnityContainer container, Type genericType)
      {
         var genericHandlerRegistrations =
             container.Registrations.Where(reg => reg.RegisteredType == genericType);

         foreach (var handlerRegistration in genericHandlerRegistrations)
         {
            if (list.All(item => item.GetType() != handlerRegistration.MappedToType))
            {
               list.Add(container.Resolve(handlerRegistration.MappedToType));
            }
         }
      }

      /// <summary>
      ///     Register all implementations of a given type for provided assembly.
      /// </summary>
      public static IUnityContainer RegisterTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
      {
         foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
         {
            var interfaces = implementation.GetInterfaces();
            foreach (var @interface in interfaces)
               container.RegisterType(@interface, implementation);
         }

         return container;
      }

      /// <summary>
      ///     Register all implementations of a given type for provided assembly.
      /// </summary>
      public static IUnityContainer RegisterNamedTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
      {
         foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
         {
            var interfaces = implementation.GetInterfaces();
            foreach (var @interface in interfaces)
               container.RegisterType(@interface, implementation, implementation.FullName);
         }

         return container;
      }

      private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
      {
         while (toCheck != null && toCheck != typeof(object))
         {
            var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == currentType)
               return true;

            toCheck = toCheck.BaseType;
         }

         return false;
      }
   }
}