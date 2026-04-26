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
    /// Resuelve la mejor versión compatible que contenga el archivo solicitado, sin tocar el Datalake.
    /// Devuelve el <see cref="VersionContext"/> de la versión elegida o null si no hay candidato válido.
    /// <para>
    /// Reglas de prioridad (sólo dentro del mismo Major):
    /// </para>
    /// <list type="number">
    ///   <item>Coincidencia exacta de Major.Minor.Patch.</item>
    ///   <item>Mismo Major.Minor con el Patch más alto.</item>
    ///   <item>Mismo Major con el Minor.Patch más altos.</item>
    /// </list>
    /// Si una versión candidata no contiene el archivo, se descarta y se prueba la siguiente.
    /// </summary>
    /// <returns>
    /// (ctx, fileId) de la versión que contiene el archivo, o null si no hay match
    /// (incluye los casos de Nodo o App no encontrados — el llamante debe validar antes si quiere distinguirlos).
    /// </returns>
    public async Task<(VersionContext Context, int FileId)?> ResolveBestVersionFileAsync(
        string nodeName, string appName, int major, int minor, int patch, string fileName)
    {
        await using var context = Factory.CreateDbContext();

        var Node = await context.Nodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Name == nodeName);
        if (Node is null) return null;

        var App = await context.Apps.AsNoTracking()
            .FirstOrDefaultAsync(a => a.NodeId == Node.Id && a.Name == appName);
        if (App is null) return null;

        // Cargamos sólo las versiones compatibles (mismo Major) y sus archivos en una sola pasada.
        // Es razonable porque el número de versiones por App es pequeño en este dominio.
        var Versions = await context.Versions.AsNoTracking()
            .Where(v => v.AppId == App.Id && v.Major == major)
            .ToListAsync();
        if (Versions.Count == 0) return null;

        var VersionIds = Versions.Select(v => v.Id).ToList();
        var FilesByVersion = await context.Files.AsNoTracking()
            .Where(f => VersionIds.Contains(f.VersionId) && f.Name == fileName)
            .ToDictionaryAsync(f => f.VersionId, f => f.Id);

        // Ordenamos los candidatos según la prioridad del fallback:
        //   1) Coincidencia exacta (Major.Minor.Patch == solicitado)
        //   2) Mismo Minor, Patch desc
        //   3) Resto del Major, Minor desc, Patch desc
        // Dentro de cada bloque la primera versión con el archivo gana.
        var Ordered = Versions
            .OrderByDescending(v => v.Major == major && v.Minor == minor && v.Patch == patch)
            .ThenByDescending(v => v.Minor == minor)
            .ThenByDescending(v => v.Minor)
            .ThenByDescending(v => v.Patch);

        foreach (var v in Ordered)
        {
            if (!FilesByVersion.TryGetValue(v.Id, out var fileId)) continue;
            var ctx = new VersionContext
            {
                NodeName = Node.Name,
                AppName = App.Name,
                VersionName = v.Name,
            };
            return (ctx, fileId);
        }

        return null;
    }

    /// <summary>
    /// Indica si existe el Nodo con el nombre indicado.
    /// </summary>
    public async Task<bool> NodeExists(string nodeName)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Nodes.AsNoTracking().AnyAsync(n => n.Name == nodeName);
    }

    /// <summary>
    /// Indica si existe la App con el nombre indicado dentro del Nodo dado.
    /// </summary>
    public async Task<bool> AppExists(string nodeName, string appName)
    {
        await using var context = Factory.CreateDbContext();
        var nodeId = await context.Nodes.AsNoTracking()
            .Where(n => n.Name == nodeName)
            .Select(n => (int?)n.Id)
            .FirstOrDefaultAsync();
        if (nodeId is null) return false;
        return await context.Apps.AsNoTracking()
            .AnyAsync(a => a.NodeId == nodeId.Value && a.Name == appName);
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
}
