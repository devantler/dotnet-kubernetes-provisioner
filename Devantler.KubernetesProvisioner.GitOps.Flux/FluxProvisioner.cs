﻿using Devantler.KubernetesProvisioner.GitOps.Core;
using Devantler.KubernetesProvisioner.Resources.Native;
using k8s;
using k8s.Models;

namespace Devantler.KubernetesProvisioner.GitOps.Flux;

/// <summary>
/// A Kubernetes GitOps provisioner using Flux.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluxProvisioner"/> class.
/// </remarks>
/// <param name="context"></param>
public class FluxProvisioner(string? context = default) : IGitOpsProvisioner
{
  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <inheritdoc/>
  public async Task PushManifestsAsync(Uri registryUri, string manifestsDirectory, string? userName = null, string? password = null, CancellationToken cancellationToken = default) =>
    await FluxCLI.Flux.PushArtifactAsync(registryUri, manifestsDirectory, cancellationToken: cancellationToken).ConfigureAwait(false);

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="ociSourceUrl"></param>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="insecure"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task BootstrapAsync(Uri ociSourceUrl, string kustomizationDirectory, bool insecure = false, CancellationToken cancellationToken = default)
  {
    await FluxCLI.Flux.InstallAsync(Context, cancellationToken).ConfigureAwait(false);
    await FluxCLI.Flux.CreateOCISourceAsync("flux-system", ociSourceUrl, insecure, Context, cancellationToken: cancellationToken)
      .ConfigureAwait(false);
    await FluxCLI.Flux.CreateKustomizationAsync("flux-system", "OCIRepository/flux-system", kustomizationDirectory, Context, wait: false,
      cancellationToken: cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task ReconcileAsync(string[] kustomizeFlow, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    using var kubernetesResourceProvisioner = new KubernetesResourceProvisioner(Context);
    await FluxCLI.Flux.ReconcileOCISourceAsync("flux-system", Context, timeout: timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
    var kustomizations = await kubernetesResourceProvisioner.CustomObjects.ListNamespacedCustomObjectAsync<V1CustomResourceDefinitionList>("kustomize.toolkit.fluxcd.io", "v1", "flux-system", "kustomizations", cancellationToken: cancellationToken).ConfigureAwait(false);
    kustomizeFlow = kustomizeFlow.Select(k => k.Replace("/", "-", StringComparison.Ordinal)).ToArray();
    kustomizations.Items = [.. kustomizations.Items.OrderBy(k => Array.IndexOf(kustomizeFlow, k.Metadata.Name))];
    foreach (var kustomization in kustomizations.Items)
    {
      await FluxCLI.Flux.ReconcileKustomizationAsync(kustomization.Metadata.Name, Context, timeout: timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
  }

  /// <summary>
  /// Uninstall Flux from the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task UninstallAsync(CancellationToken cancellationToken = default) =>
    await FluxCLI.Flux.UninstallAsync(Context, cancellationToken).ConfigureAwait(false);
}
