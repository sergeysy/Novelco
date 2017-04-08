using System;
using System.ServiceModel;
using Logisto.Models;
using Microsoft.AspNet.Identity;


namespace Logisto.Identity
{
	public interface IContainerBuilder<out T>
	{
		T Container { get; }

		IContainerBuilder<T> Register<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService;

		IContainerBuilder<T> RegisterSingle<TService, TImplementation>()
			where TService : class
			where TImplementation : class, TService;

		IContainerBuilder<T> Register<TService>(Func<TService> func) where TService : class;

		IContainerBuilder<T> RegisterAll<TService>(params Type[] types) where TService : class;

		TService GetInstance<TService>() where TService : class;

		IContainerBuilder<T> RegisterServiceClient<TService>(Func<ChannelFactory<TService>, TService> func) where TService : class;

	}

	public static class IdentityContainerExtension
	{
		public static IContainerBuilder<T> RegisterIdentity<T>(this IContainerBuilder<T> container)
		{
			return container
				.Register<IRoleStore<IdentityRole, int>, RoleStore>()
				.Register<RoleManager, RoleManager>()
				.Register<SignInHelper, SignInHelper>()
				.Register<EmailService, EmailService>();
		}
	}
}
