using System.Text.RegularExpressions;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Map;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using OneOf;
using OneOf.Types;

namespace Acorn.Database.Repository;

public class DataFileRepository : IDataFileRepository
{
    private readonly string _ecfFile = "Data/Pub/dat001.ecf";
    private readonly string _eifFile = "Data/Pub/dat001.eif";
    private readonly string _enfFile = "Data/Pub/dtn001.enf";
    private readonly string _esfFile = "Data/Pub/dsl001.esf";

    public DataFileRepository()
    {
        Ecf.Deserialize(new EoReader(File.ReadAllBytes(_ecfFile)));
        Eif.Deserialize(new EoReader(File.ReadAllBytes(_eifFile)));
        Enf.Deserialize(new EoReader(File.ReadAllBytes(_enfFile)));
        Esf.Deserialize(new EoReader(File.ReadAllBytes(_esfFile)));
        Maps = Directory.GetFiles("Data/Maps/").ToList().Where(f => Regex.IsMatch(f, @"\d+\.emf")).Select(mapFile =>
        {
            var emf = new Emf();
            emf.Deserialize(new EoReader(File.ReadAllBytes(mapFile)));
            var id = int.Parse(new FileInfo(mapFile).Name.Split('.')[0]);
            return new MapWithId(id, emf);
        }).ToList();
    }

    public Ecf Ecf { get; } = new();
    public Eif Eif { get; } = new();
    public Enf Enf { get; } = new();
    public Esf Esf { get; } = new();
    public IEnumerable<MapWithId> Maps { get; }
}

public static class EcfExtension
{
    public static OneOf<Success<EcfRecord>, Error<string>> GetClass(this Ecf ecf, int id)
    {
        if (id > ecf.Classes.Count)
        {
            return new Error<string>($"{id} is greater than the class count ({ecf.Classes.Count})");
        }

        var @class = ecf.Classes[id];
        return new Success<EcfRecord>(@class);
    }
}

public static class EnfExtension
{
    public static OneOf<Success<EnfRecord>, Error<string>> GetNpc(this Enf enf, int id)
    {
        if (id > enf.Npcs.Count)
        {
            return new Error<string>($"{id} is greater than the npc count ({enf.Npcs.Count})");
        }

        var npc = enf.Npcs[id];
        return new Success<EnfRecord>(npc);
    }
}

public static class EifExtension
{
    public static OneOf<Success<EifRecord>, Error<string>> GetItem(this Eif eif, int id)
    {
        if (id > eif.Items.Count)
        {
            return new Error<string>($"{id} is greater than the item count ({eif.Items.Count})");
        }

        var item = eif.Items[id];
        return new Success<EifRecord>(item);
    }
}

public static class EsfExtension
{
    public static OneOf<Success<EsfRecord>, Error<string>> GetSkill(this Esf esf, int id)
    {
        if (id > esf.Skills.Count)
        {
            return new Error<string>($"{id} is greater than the skill count ({esf.Skills.Count})");
        }

        var skill = esf.Skills[id];
        return new Success<EsfRecord>(skill);
    }
}

public record MapWithId(int Id, Emf Map);

public interface IDataFileRepository
{
    Ecf Ecf { get; }
    Eif Eif { get; }
    Enf Enf { get; }
    Esf Esf { get; }
    IEnumerable<MapWithId> Maps { get; }
}