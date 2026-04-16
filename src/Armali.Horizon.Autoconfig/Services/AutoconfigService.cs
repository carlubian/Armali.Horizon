using System.Text;
using Armali.Horizon.Autoconfig.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Autoconfig.Services;

public class AutoconfigService
{
    private readonly IDbContextFactory<AutoconfigDbContext> Factory;
    private readonly AutoconfigDatalakeService DatalakeService;

    public AutoconfigService(IDbContextFactory<AutoconfigDbContext> factory, AutoconfigDatalakeService datalakeService)
    {
        Factory = factory;
        DatalakeService = datalakeService;
    }
    
    public async Task<List<AutoconfigNode>> GetNodes()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Nodes
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<NodeStats> GetNodeStats(int nodeId)
    {
        await using var context = Factory.CreateDbContext();
        var result = new NodeStats();
        
        result.AppCount = await context.Apps
            .Where(a => a.NodeId == nodeId)
            .AsNoTracking()
            .CountAsync();
        result.TotalKbSize = await context.Files
            .Where(f => context.Versions
                .Where(v => context.Apps
                    .Where(a => a.NodeId == nodeId)
                    .Select(a => a.Id)
                    .Contains(v.AppId))
                .Select(v => v.Id)
                .Contains(f.VersionId))
            .AsNoTracking()
            .SumAsync(f => f.KbSize);

        return result;
    }
    
    public async Task AddNode(AutoconfigNode entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Nodes.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateNode(AutoconfigNode entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Nodes.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteNode(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Nodes.FindAsync(id);
        if (Entity == null) return;
        
        // Obtener todas las apps de este nodo para limpiar el Datalake
        var Apps = await context.Apps.Where(a => a.NodeId == id).AsNoTracking().ToListAsync();
        foreach (var app in Apps)
        {
            var Versions = await context.Versions.Where(v => v.AppId == app.Id).AsNoTracking().ToListAsync();
            foreach (var version in Versions)
            {
                var Files = await context.Files.Where(f => f.VersionId == version.Id).ToListAsync();
                foreach (var file in Files)
                {
                    await DatalakeService.DeleteFileAsync(Entity.Name, app.Name, version.Name, file.Name);
                }
                context.Files.RemoveRange(Files);
            }
            context.Versions.RemoveRange(await context.Versions.Where(v => v.AppId == app.Id).ToListAsync());
        }
        context.Apps.RemoveRange(await context.Apps.Where(a => a.NodeId == id).ToListAsync());
        
        context.Nodes.Remove(Entity);
        await context.SaveChangesAsync();
    }
    
    public async Task<List<AutoconfigApp>> GetApps()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Apps
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<AppStats> GetAppStats(int appId)
    {
        await using var context = Factory.CreateDbContext();
        var result = new AppStats();
        
        result.VersionCount = await context.Versions
            .Where(a => a.AppId == appId)
            .AsNoTracking()
            .CountAsync();
        result.TotalKbSize = await context.Files
            .Where(f => context.Versions
                .Where(a => a.AppId == appId)
                .Select(v => v.Id)
                .Contains(f.VersionId))
            .AsNoTracking()
            .SumAsync(f => f.KbSize);

        return result;
    }
    
    public async Task<List<AutoconfigApp>> GetAppsIn(int nodeId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Apps
            .Where(a => a.NodeId == nodeId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddApp(AutoconfigApp entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Apps.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateApp(AutoconfigApp entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Apps.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteApp(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Apps.FindAsync(id);
        if (Entity == null) return;
        
        // Resolver nombre del nodo para rutas del Datalake
        var Node = await context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == Entity.NodeId);
        
        // Eliminar ficheros del Datalake y de la BD para todas las versiones
        var Versions = await context.Versions.Where(v => v.AppId == id).AsNoTracking().ToListAsync();
        foreach (var version in Versions)
        {
            var Files = await context.Files.Where(f => f.VersionId == version.Id).ToListAsync();
            foreach (var file in Files)
            {
                if (Node != null)
                    await DatalakeService.DeleteFileAsync(Node.Name, Entity.Name, version.Name, file.Name);
            }
            context.Files.RemoveRange(Files);
        }
        context.Versions.RemoveRange(await context.Versions.Where(v => v.AppId == id).ToListAsync());
        
        context.Apps.Remove(Entity);
        await context.SaveChangesAsync();
    }
    
    public async Task<List<AutoconfigVersion>> GetVersions()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Versions
            .OrderByDescending(v => v.Major)
            .ThenByDescending(v => v.Minor)
            .ThenByDescending(v => v.Patch)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigVersion>> GetVersionsIn(int appId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Versions
            .Where(a => a.AppId == appId)
            .OrderByDescending(v => v.Major)
            .ThenByDescending(v => v.Minor)
            .ThenByDescending(v => v.Patch)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddVersion(AutoconfigVersion entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Versions.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateVersion(AutoconfigVersion entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Versions.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteVersion(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Versions.FindAsync(id);
        if (Entity == null) return;
        
        // Resolver nombres para rutas del Datalake
        var App = await context.Apps.AsNoTracking().FirstOrDefaultAsync(a => a.Id == Entity.AppId);
        var Node = App != null 
            ? await context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == App.NodeId) 
            : null;
        
        // Eliminar ficheros del Datalake y de la BD
        var Files = await context.Files.Where(f => f.VersionId == id).ToListAsync();
        foreach (var file in Files)
        {
            if (Node != null && App != null)
                await DatalakeService.DeleteFileAsync(Node.Name, App.Name, Entity.Name, file.Name);
        }
        context.Files.RemoveRange(Files);
        
        context.Versions.Remove(Entity);
        await context.SaveChangesAsync();
    }
    
    // ── Files ──────────────────────────────────────────────────
    
    public async Task<List<AutoconfigFile>> GetFiles()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Files
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigFile>> GetFilesIn(int versionId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Files
            .Where(a => a.VersionId == versionId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<AutoconfigFile?> GetFileById(int fileId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId);
    }
    
    /// <summary>
    /// Resuelve los nombres de la jerarquía Nodo → App → Versión a partir del VersionId.
    /// </summary>
    public async Task<VersionContext?> GetVersionContext(int versionId)
    {
        await using var context = Factory.CreateDbContext();
        var Version = await context.Versions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == versionId);
        if (Version == null) return null;
        
        var App = await context.Apps.AsNoTracking().FirstOrDefaultAsync(a => a.Id == Version.AppId);
        if (App == null) return null;
        
        var Node = await context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == App.NodeId);
        if (Node == null) return null;
        
        return new VersionContext
        {
            NodeName = Node.Name,
            AppName = App.Name,
            VersionName = Version.Name
        };
    }
    
    /// <summary>
    /// Añade un fichero: inserta el registro en BD y sube el contenido al Datalake.
    /// </summary>
    public async Task AddFile(AutoconfigFile entity, Stream content, VersionContext ctx)
    {
        await DatalakeService.UploadFileAsync(content, ctx.NodeName, ctx.AppName, ctx.VersionName, entity.Name);
        
        await using var context = Factory.CreateDbContext();
        context.Files.Add(entity);
        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Elimina un fichero: borra del Datalake y del registro en BD.
    /// </summary>
    public async Task DeleteFile(int fileId, VersionContext ctx)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Files.FindAsync(fileId);
        if (Entity == null) return;
        
        await DatalakeService.DeleteFileAsync(ctx.NodeName, ctx.AppName, ctx.VersionName, Entity.Name);
        
        context.Files.Remove(Entity);
        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Descarga el contenido de un fichero del Datalake como texto UTF-8.
    /// </summary>
    public async Task<string> GetFileContent(VersionContext ctx, string fileName)
    {
        var FileStream = await DatalakeService.GetFileAsync(ctx.NodeName, ctx.AppName, ctx.VersionName, fileName);
        using var Reader = new StreamReader(FileStream, Encoding.UTF8);
        return await Reader.ReadToEndAsync();
    }
    
    /// <summary>
    /// Sobrescribe el contenido de un fichero en el Datalake y actualiza KbSize en BD.
    /// </summary>
    public async Task UpdateFileContent(int fileId, string content, VersionContext ctx, string fileName)
    {
        var Bytes = Encoding.UTF8.GetBytes(content);
        await using var Stream = new MemoryStream(Bytes);
        await DatalakeService.UploadFileAsync(Stream, ctx.NodeName, ctx.AppName, ctx.VersionName, fileName);
        
        // Actualizar tamaño en BD
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Files.FindAsync(fileId);
        if (Entity != null)
        {
            Entity.KbSize = Bytes.Length / 1024;
            context.Files.Update(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    // ── FindValidFile ──────────────────────────────────────────
    
    /// <summary>
    /// Busca un fichero de configuración aplicando fallback de versión compatible:
    /// 1) Coincidencia exacta de Major.Minor.Patch
    /// 2) Misma Major.Minor, versión más reciente (desc por Patch)
    /// 3) Misma Major, versión más reciente (desc por Minor, Patch)
    /// No cruza entre distintas Major ya que se consideran incompatibles.
    /// </summary>
    public async Task<FindFileResult?> FindValidFile(string nodeName, string appName, string version, string fileName)
    {
        // Parsear versión solicitada
        var Parts = version.Split('.');
        if (Parts.Length != 3
            || !int.TryParse(Parts[0], out var Major)
            || !int.TryParse(Parts[1], out var Minor)
            || !int.TryParse(Parts[2], out var Patch))
            return null;
        
        await using var context = Factory.CreateDbContext();
        
        // Resolver Node y App por nombre
        var Node = await context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.Name == nodeName);
        if (Node == null) return null;
        
        var App = await context.Apps.AsNoTracking().FirstOrDefaultAsync(a => a.Name == appName && a.NodeId == Node.Id);
        if (App == null) return null;
        
        // Obtener todas las versiones de esta App con la misma Major, ordenadas de más reciente a más antigua
        var Candidates = await context.Versions
            .Where(v => v.AppId == App.Id && v.Major == Major)
            .OrderByDescending(v => v.Minor)
            .ThenByDescending(v => v.Patch)
            .AsNoTracking()
            .ToListAsync();
        
        if (Candidates.Count == 0) return null;
        
        // Estrategia de búsqueda en cascada: exacta → Major.Minor → Major
        var SearchPasses = new List<Func<AutoconfigVersion, bool>>
        {
            v => v.Major == Major && v.Minor == Minor && v.Patch == Patch,
            v => v.Major == Major && v.Minor == Minor,
            v => v.Major == Major
        };
        
        foreach (var Predicate in SearchPasses)
        {
            foreach (var Candidate in Candidates.Where(Predicate))
            {
                var File = await context.Files
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.VersionId == Candidate.Id && f.Name == fileName);
                
                if (File == null) continue;
                
                // Fichero encontrado — descargar contenido del Datalake
                var Ctx = new VersionContext
                {
                    NodeName = Node.Name,
                    AppName = App.Name,
                    VersionName = Candidate.Name
                };
                var Content = await GetFileContent(Ctx, fileName);
                return new FindFileResult
                {
                    Content = Content,
                    ResolvedVersion = Candidate.Name
                };
            }
        }
        
        return null;
    }
}
