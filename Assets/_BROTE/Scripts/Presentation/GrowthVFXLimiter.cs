using System;

/// <summary>Three small expiring leases. Clock supplied by caller for deterministic tests.</summary>
public sealed class GrowthVFXLimiter
{
    public struct Lease
    {
        public int Slot;
        public long Version;
    }
    private readonly float[] expires;
    private readonly long[] versions;
    private long serial;
    public int Capacity => expires.Length;
    public GrowthVFXLimiter(int capacity = 3)
    {
        expires = new float[Math.Max(1, Math.Min(3, capacity))];
        versions = new long[expires.Length];
    }
    public bool TryAcquire(float now, float lifetime, out Lease lease)
    {
        lease = default;
        if (float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(lifetime) || float.IsInfinity(lifetime) || lifetime <= 0f) return false;
        for (int i = 0; i < expires.Length; i++)
        {
            if (versions[i] != 0 && expires[i] > now) continue;
            versions[i] = ++serial;
            expires[i] = now + lifetime;
            lease = new Lease { Slot = i, Version = versions[i] };
            return true;
        }
        return false;
    }
    public void Release(Lease lease)
    {
        if (lease.Version == 0 || lease.Slot < 0 || lease.Slot >= versions.Length || versions[lease.Slot] != lease.Version) return;
        versions[lease.Slot] = 0; expires[lease.Slot] = 0f;
    }
    public int ActiveCount(float now)
    {
        int count = 0;
        for (int i = 0; i < versions.Length; i++) if (versions[i] != 0 && expires[i] > now) count++;
        return count;
    }
    public void Clear() { Array.Clear(expires, 0, expires.Length); Array.Clear(versions, 0, versions.Length); }
}
