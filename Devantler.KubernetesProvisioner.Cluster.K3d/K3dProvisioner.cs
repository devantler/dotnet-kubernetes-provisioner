﻿using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.Cluster.K3d;

/// <summary>
/// A Kubernetes cluster provisioner for K3d.
/// </summary>
public class K3dProvisioner : IKubernetesClusterProvisioner
{
  /// <inheritdoc />
  public async Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken = default) =>
    await K3dCLI.K3d.DeleteClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken = default) =>
    await K3dCLI.K3d.GetClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default) =>
    await K3dCLI.K3d.ListClustersAsync(cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken = default) =>
    await K3dCLI.K3d.CreateClusterAsync(clusterName, configPath, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task StartAsync(string clusterName, CancellationToken cancellationToken = default) =>
    await K3dCLI.K3d.StartClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public Task StopAsync(string clusterName, CancellationToken cancellationToken = default) =>
    K3dCLI.K3d.StopClusterAsync(clusterName, cancellationToken);
}
