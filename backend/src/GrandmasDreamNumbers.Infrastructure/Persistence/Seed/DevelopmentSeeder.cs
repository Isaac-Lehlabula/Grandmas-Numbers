using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only seed data. Runs on startup in the Development
/// environment, and only if the DreamSymbols table is completely empty -
/// it never overwrites or upgrades an already-seeded database (see
/// docs/cheat-sheet-import.md).
///
/// <see cref="CheatSheet"/> is the traditional numbers-dream book supplied
/// by the project owner (numbers 1-52, each with its associated dream
/// symbols). It's transcribed faithfully: symbol names that appear under
/// two different numbers in the source (e.g. "Coffin" under both 16 and
/// 29) are merged into one symbol with both lucky numbers rather than
/// duplicated. A handful of obvious typos were corrected so exact-text
/// matching works against correctly-spelled dream descriptions (Suprise ->
/// Surprise, Lightening -> Lightning, Sjhambok -> Sjambok), and two
/// comma/and-joined source entries were split into two symbols each (row
/// 17 "Pearls, Diamond"; row 46 "Sea and Scissors" -> Sea sand + Scissors,
/// per a second alphabetized cross-reference list the owner also
/// supplied). No interpretive "traditional meaning" text was invented
/// beyond the number association itself - <see cref="CreateSymbol"/>
/// generates plain, factual description/meaning text rather than
/// fabricating cultural interpretations that weren't supplied.
///
/// The numbers were cross-checked against that second, alphabetized list
/// (same source material, reorganized). Everything matched except a
/// handful of entries where the alphabetized list contradicted itself
/// (e.g. "Anything Dirty 46" vs. its own later "Dirty (anything) 34") -
/// in each such case the number matching the numbered table (and the
/// alphabetized list's own second listing) was kept: Anything dirty stays
/// 34, Big house stays 25, Big stick stays 7, Right eye stays 18. One
/// unresolvable conflict (Frog: 3 in the numbered table vs. 30 in the
/// alphabetized list, no self-corroboration either way) was resolved by
/// the project owner in favor of 3. A bare "Bird 19" entry only in the
/// alphabetized list (separate from "Big bird 19") was deliberately left
/// out as likely redundant.
/// </summary>
public static class DevelopmentSeeder
{
    private static readonly string[] Roles = ["Admin", "User"];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(roleManager);
        await SeedDreamSymbolsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task SeedDreamSymbolsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.DreamSymbols.AnyAsync(cancellationToken))
        {
            return;
        }

        var symbols = CheatSheet.Select(entry => CreateSymbol(entry.Name, entry.Numbers)).ToArray();

        dbContext.DreamSymbols.AddRange(symbols);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DreamSymbol CreateSymbol(string name, int[] numbers)
    {
        var symbol = new DreamSymbol
        {
            Name = name,
            Description = $"\"{name}\" appearing in the dream.",
            TraditionalMeaning = numbers.Length > 1
                ? $"Associated with numbers {string.Join(" and ", numbers)} in this dream-number tradition."
                : $"Associated with number {numbers[0]} in this dream-number tradition.",
        };

        symbol.LuckyNumbers = numbers
            .Select((n, index) => new LuckyNumber { Number = n, IsPrimary = index == 0, DreamSymbolId = symbol.Id })
            .ToList();

        return symbol;
    }

    // Number -> dream symbols, transcribed from the source dream book.
    // Symbol names repeated under two numbers in the source (Coffin,
    // Rain, Thief, Crown, Fight, Hole) are merged here into one entry
    // with both numbers rather than listed twice.
    private static readonly (string Name, int[] Numbers)[] CheatSheet =
    [
        // 1
        ("King", [1]), ("Human blood", [1]), ("White man", [1]), ("Left eye", [1]),
        // 2
        ("Monkey", [2]), ("Native", [2]), ("A Spirit", [2]), ("Chief", [2]), ("Copper", [2]), ("Money", [2]), ("Jockey", [2]),
        // 3
        ("Sea Water", [3]), ("Accident", [3]), ("Frog", [3]), ("Sailor", [3]), ("Sex", [3]),
        // 4
        ("Dead man", [4]), ("Turkey", [4]), ("Small Fortune", [4]), ("Bed", [4]),
        // 5
        ("Tiger", [5]), ("Fight", [5, 31]), ("Strong Man", [5]),
        // 6
        ("Ox Blood", [6]), ("Gentleman", [6]), ("Milk", [6]),
        // 7
        ("Lion", [7]), ("Thief", [7, 28]), ("Big stick", [7]), ("Chickens", [7]),
        // 8
        ("Pig", [8]), ("Drunken man", [8]), ("Loafer", [8]), ("Fat man", [8]), ("Chinese king", [8]),
        // 9
        ("Moon", [9]), ("Baby", [9]), ("Hole", [9, 24]), ("Owl", [9]), ("Devil", [9]), ("Pumpkin", [9]), ("Anything round", [9]),
        // 10
        ("Eggs", [10]), ("Train", [10]), ("Boat", [10]), ("Grave", [10]), ("Anything Oval", [10]),
        // 11
        ("Carriage", [11]), ("Wood", [11]), ("Tree", [11]), ("Furniture", [11]), ("Bicycle", [11]), ("Flowers", [11]),
        // 12
        ("Dead woman", [12]), ("Ducks", [12]), ("Small fire", [12]), ("Chinese Queen", [12]),
        // 13
        ("Big fish", [13]), ("Ghosts", [13]), ("Spirits", [13]),
        // 14
        ("Old woman", [14]), ("Fox", [14]), ("Detective", [14]), ("Nurse", [14]), ("Native woman", [14]),
        // 15
        ("Bad woman", [15]), ("Prostitute", [15]), ("Canary", [15]), ("White horse", [15]), ("Small knife", [15]),
        // 16
        ("Small house", [16]), ("Coffin", [16, 29]), ("Pigeon", [16]), ("Young woman", [16]), ("Paper money", [16]), ("Letter", [16]),
        // 17 (source: "Pearls, Diamond" split into two entries)
        ("Diamond woman", [17]), ("Queen", [17]), ("Pearls", [17]), ("Diamond", [17]), ("Stars", [17]), ("White woman", [17]),
        // 18
        ("Silver money", [18]), ("Servant girl", [18]), ("Right eye", [18]), ("Butterfly", [18]), ("Hook", [18]), ("Rain", [18, 29]),
        // 19
        ("Little girl", [19]), ("Smoke", [19]), ("Bread", [19]), ("Big bird", [19]), ("Left hand", [19]),
        // 20
        ("Cat", [20]), ("Sky", [20]), ("Handkerchief", [20]), ("Body", [20]), ("Music", [20]), ("Minister", [20]), ("Naked woman", [20]),
        // 21
        ("Old man", [21]), ("Stranger", [21]), ("Fisherman", [21]), ("Elephant", [21]), ("Knife", [21]), ("Nose", [21]), ("Teeth", [21]),
        // 22
        ("Rats", [22]), ("Motor car", [22]), ("Big ship", [22]), ("Left foot", [22]), ("Shoes", [22]),
        // 23
        ("Horse", [23]), ("Doctor", [23]), ("Head", [23]), ("Hair", [23]), ("Crown", [23, 26]),
        // 24
        ("Mouth", [24]), ("Wild cat", [24]), ("Vixen", [24]), ("Lioness", [24]), ("Purse", [24]),
        // 25
        ("Big house", [25]), ("Church", [25]), ("Boxer", [25]), ("Hospital", [25]),
        // 26
        ("Bees", [26]), ("Bad man", [26]), ("Bush", [26]), ("General", [26]), ("Funeral", [26]), ("Madman", [26]),
        // 27
        ("Dog", [27]), ("Policeman", [27]), ("Newborn baby", [27]), ("Medicine", [27]), ("Sad news", [27]),
        // 28
        ("Sardines", [28]), ("Small fish", [28]), ("Right foot", [28]), ("Surprise", [28]), ("Small child", [28]),
        // 29
        ("Small water", [29]), ("Tears", [29]), ("Big knife", [29]), ("Right hand", [29]),
        // 30
        ("Fowl", [30]), ("Graveyard", [30]), ("Sun", [30]), ("Throat", [30]), ("Indian", [30]), ("Forest", [30]),
        // 31
        ("Big fire", [31]), ("Bishop", [31]), ("Big spirit", [31]), ("Feathers", [31]), ("Woman", [31]),
        // 32
        ("Gold money", [32]), ("Dirty woman", [32]), ("Snake", [32]),
        // 33
        ("Little boy", [33]), ("Spider", [33]),
        // 34
        ("Meat", [34]), ("Human dung", [34]), ("Anything dirty", [34]), ("Cripple", [34]), ("Tramp", [34]),
        // 35
        ("Clothes", [35]), ("Sheep", [35]), ("Big hole", [35]), ("Big grave", [35]),
        // 36
        ("Shrimp", [36]), ("Stick", [36]), ("Admiral", [36]), ("Cigars", [36]), ("Gum", [36]),
        // 37
        ("Arrow", [37]), ("Lawyer", [37]), ("Treasure", [37]), ("Cooking", [37]), ("Stream", [37]),
        // 38
        ("Crocodile", [38]), ("Balloons", [38]), ("Sjambok", [38]), ("Fireworks", [38]), ("Stadium", [38]),
        // 39
        ("Sangoma", [39]), ("Soccer team", [39]), ("Tattoos", [39]), ("Bloodshed", [39]), ("Teacher", [39]),
        // 40
        ("Birth", [40]), ("Clock", [40]), ("Snail", [40]), ("Dwarf", [40]), ("River", [40]), ("Traditional healer", [40]),
        // 41
        ("Cattle", [41]), ("Planets", [41]), ("Cave", [41]), ("Desert", [41]), ("Monster", [41]),
        // 42
        ("Tornado", [42]), ("Spear", [42]), ("Umbrella", [42]), ("Camel", [42]), ("Door", [42]),
        // 43
        ("Army", [43]), ("Thunder", [43]), ("Astronaut", [43]), ("Rabbit", [43]), ("Turtle", [43]),
        // 44
        ("Shark", [44]), ("Stud farm", [44]), ("Body builder", [44]), ("Injury", [44]), ("Mud", [44]),
        // 45
        ("Football", [45]), ("Computers", [45]), ("Jewellery", [45]), ("Wrestler", [45]), ("Storm", [45]),
        // 46 (source: "Sea and Scissors" split into two entries; "Sea"
        // clarified to "Sea sand" per the alphabetized cross-reference list)
        ("Ambulance", [46]), ("Beard", [46]), ("Sea sand", [46]), ("Scissors", [46]), ("Key", [46]),
        // 47
        ("Stallion", [47]), ("Kite", [47]), ("TV", [47]), ("Lightning", [47]), ("Carnival", [47]), ("Hut", [47]),
        // 48
        ("Clown", [48]), ("Rainbow", [48]), ("Nightmare", [48]), ("Whale", [48]), ("Wealth", [48]),
        // 49
        ("Shebeen", [49]), ("Circus", [49]), ("Chocolate", [49]), ("Space ship", [49]),
        // 50
        ("Bathroom", [50]), ("Magic", [50]), ("Revolution", [50]), ("Trap", [50]), ("Wheat", [50]),
        // 51
        ("Car", [51]), ("Carrot", [51]), ("Orange", [51]), ("Vulture", [51]), ("Wasp", [51]),
        // 52
        ("Brush", [52]), ("Eagle", [52]), ("Salon", [52]), ("Shower", [52]), ("Trumpet", [52]),
    ];
}
