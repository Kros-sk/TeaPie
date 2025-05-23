﻿namespace TeaPie.Http.Auth;

public static class TeaPieAuthenticationExtensions
{
    /// <summary>
    /// Registers an authentication provider with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the authentication provider will be registered.</param>
    /// <param name="authenticationProvider">The authentication provider to register.</param>
    public static void RegisterAuthProvider(
        this TeaPie teaPie, string name, IAuthProvider authenticationProvider)
        => teaPie._authenticationProviderRegistry.Register(name, authenticationProvider);

    /// <summary>
    /// Registers an authentication provider with the specified <paramref name="name"/> and sets it as the default provider.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the authentication provider will be registered.</param>
    /// <param name="authenticationProvider">The authentication provider to register.</param>
    public static void RegisterDefaultAuthProvider(this TeaPie teaPie, string name, IAuthProvider authenticationProvider)
    {
        teaPie.RegisterAuthProvider(name, authenticationProvider);
        SetDefaultProvider(teaPie, name);
    }

    /// <summary>
    /// Sets the default authentication provider for all requests.
    /// A different authentication provider can still be specified for individual requests using a directive.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of a previously registered authentication provider to set as default.</param>
    /// <exception cref="InvalidOperationException">Thrown if no authentication provider is registered with the specified
    /// <paramref name="name"/>.</exception>
    public static void SetDefaultAuthProvider(this TeaPie teaPie, string name)
    {
        if (!teaPie._authenticationProviderRegistry.IsRegistered(name))
        {
            throw new InvalidOperationException("Cannot set the default authentication provider because it is not registered.");
        }

        SetDefaultProvider(teaPie, name);
    }

    private static void SetDefaultProvider(TeaPie teaPie, string name)
        => teaPie._authProviderAccessor.DefaultProvider = teaPie._authenticationProviderRegistry.Get(name);
}
