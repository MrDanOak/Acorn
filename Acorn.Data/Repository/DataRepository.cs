using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Map;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using OneOf;
using OneOf.Types;
using System.Text.RegularExpressions;

namespace Acorn.Data.Repository;
public class DataRepository : IDataRepository
{ 
    private readonly Ecf _ecf = new();
    private readonly Eif _eif = new();
    private readonly Enf _enf = new();
    private readonly List<MapWithId> _maps = new();

    public string EcfFile = "Data/Pub/dat001.ecf";
    public string EifFile = "Data/Pub/dat001.eif";
    public string EnfFile = "Data/Pub/dtn001.enf";

    public DataRepository()
    {
        _ecf.Deserialize(new EoReader(File.ReadAllBytes(EcfFile)));
        _eif.Deserialize(new EoReader(File.ReadAllBytes(EifFile)));
        _enf.Deserialize(new EoReader(File.ReadAllBytes(EifFile)));
        _maps = Directory.GetFiles("Data/Maps/").ToList().Where(f => Regex.IsMatch(f, @"\d+\.emf")).Select(mapFile =>
        {
            var emf = new Emf();
            emf.Deserialize(new EoReader(File.ReadAllBytes(mapFile)));
            var id = int.Parse(new FileInfo(mapFile).Name.Split('.')[0]);
            return new MapWithId(id, emf);
        }).ToList();
    }

    public IEnumerable<EcfRecord> GetClasses() => _ecf.Classes;

    public OneOf<Success<EcfRecord>, Error<string>> GetClass(int id)
    {
        if (id > _ecf.Classes.Count)
        {
            return new Error<string>($"{id} is greater than the class count ({_ecf.Classes.Count})");
        }
        var @class = _ecf.Classes[id];
        return new Success<EcfRecord>(@class);
    }

    public IEnumerable<EifRecord> GetItems() => _eif.Items;

    public OneOf<Success<EifRecord>, Error<string>> GetItem(int id)
    {
        if (id > _eif.Items.Count)
        {
            return new Error<string>($"{id} is greater than the item count ({_eif.Items.Count})");
        }
        var item = _eif.Items[id];
        return new Success<EifRecord>(item);
    }

    public IEnumerable<EnfRecord> GetNpcs() => _enf.Npcs;

    public OneOf<Success<EnfRecord>, Error<string>> GetNpc(int id)
    {
        if (id > _enf.Npcs.Count)
        {
            return new Error<string>($"{id} is greater than the npc count ({_enf.Npcs.Count})");
        }
        var npc = _enf.Npcs[id];
        return new Success<EnfRecord>(npc);
    }

    public IEnumerable<MapWithId> GetMaps() => _maps;
    public OneOf<Success<MapWithId>, NotFound> GetMap(int id)
    {
        var map = _maps.FirstOrDefault(m => m.Id == id);
        if (map is null)
        {
            return new NotFound();
        }

        return new Success<MapWithId>(map);
    }
}

public record MapWithId(int Id, Emf Map);

public interface IDataRepository
{
    OneOf<Success<EcfRecord>, Error<string>> GetClass(int id);
    IEnumerable<EcfRecord> GetClasses();
    OneOf<Success<EifRecord>, Error<string>> GetItem(int id);
    IEnumerable<EifRecord> GetItems();
    OneOf<Success<EnfRecord>, Error<string>> GetNpc(int id);
    IEnumerable<EnfRecord> GetNpcs();
    IEnumerable<MapWithId> GetMaps();
    OneOf<Success<MapWithId>, NotFound> GetMap(int id);
}