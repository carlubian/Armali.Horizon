using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris;

public class SegarisDbContext(DbContextOptions<SegarisDbContext> options) : DbContext(options)
{
    // CAPEX module
    public DbSet<CapexCategory> CapexCategories { get; set; }
    public DbSet<CapexStatus> CapexStatuses { get; set; }
    public DbSet<CapexEntity> CapexEntities { get; set; }
    
    // OPEX module
    public DbSet<OpexCategory> OpexCategories { get; set; }
    public DbSet<OpexStatus> OpexStatuses { get; set; }
    public DbSet<OpexEntity> OpexEntities { get; set; }
    public DbSet<OpexSubEntity> OpexSubEntities { get; set; }
    
    // INVENTORY module
    public DbSet<InvVendorStatus> InvVendorStatuses { get; set; }
    public DbSet<InvVendorEntity> InvVendorEntities { get; set; }
    public DbSet<InvItemCategory> InvItemCategories { get; set; }
    public DbSet<InvItemStatus> InvItemStatuses { get; set; }
    public DbSet<InvItemEntity> InvItemEntities { get; set; }
    public DbSet<InvOrderStatus> InvOrderStatuses { get; set; }
    public DbSet<InvOrderEntity> InvOrderEntities { get; set; }
    public DbSet<InvOrderSubEntity> InvOrderSubEntities { get; set; }
    
    // ASSET module
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<AssetStatus> AssetStatuses { get; set; }
    public DbSet<AssetEntity> AssetEntities { get; set; }
    
    // TRAVEL module
    public DbSet<TravelCategory> TravelCategories { get; set; }
    public DbSet<TravelStatus> TravelStatuses { get; set; }
    public DbSet<TravelCostCenter> TravelCostCenters { get; set; }
    public DbSet<TravelEntity> TravelEntities { get; set; }
    public DbSet<TravelSubEntityCategory> TravelSubEntityCategories { get; set; }
    public DbSet<TravelSubEntity> TravelSubEntities { get; set; }
    
    // PROJECT module
    public DbSet<ProjectSubEntityCategory> ProjectSubEntityCategories { get; set; }
    public DbSet<ProjectProgram> ProjectPrograms { get; set; }
    public DbSet<ProjectAxis> ProjectAxis { get; set; }
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<ProjectEntity> ProjectEntities { get; set; }
    public DbSet<ProjectSubEntity> ProjectSubEntities { get; set; }
    public DbSet<ProjectRiskCategory> ProjectRiskCategories { get; set; }
    public DbSet<ProjectRiskElement> ProjectRiskElements { get; set; }
    
    // ARCHIVE module
    public DbSet<ArchiveCategory> ArchiveCategories { get; set; }
    public DbSet<ArchiveStatus> ArchiveStatuses { get; set; }
    public DbSet<ArchiveEntity> ArchiveEntities { get; set; }
    
    // MAINT module
    public DbSet<MaintCategory> MaintCategories { get; set; }
    public DbSet<MaintStatus> MaintStatuses { get; set; }
    public DbSet<MaintEntity> MaintEntities { get; set; }
    
    // FIREBIRD module
    public DbSet<FirebirdCategory> FirebirdCategories { get; set; }
    public DbSet<FirebirdStatus> FirebirdStatuses { get; set; }
    public DbSet<FirebirdEntity> FirebirdEntities { get; set; }
    public DbSet<FirebirdSubEntity> FirebirdSubEntities { get; set; }
    
    // CLOTHES module
    public DbSet<ClothesCategory> ClothesCategories { get; set; }
    public DbSet<ClothesStatus> ClothesStatuses { get; set; }
    public DbSet<ClothesWashType> ClothesWashTypes { get; set; }
    public DbSet<ClothesColor> ClothesColors { get; set; }
    public DbSet<ClothesColorStyle> ClothesColorStyles { get; set; }
    public DbSet<ClothesEntity> ClothesEntities { get; set; }
    public DbSet<ClothesColorAssignment> ClothesColorAssignments { get; set; }
    
    // MOOD module
    public DbSet<MoodCategory> MoodCategories { get; set; }
    public DbSet<MoodEntity> MoodEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CAPEX module seed data
        modelBuilder.Entity<CapexCategory>().HasData(
            new CapexCategory { Id = 1, Name = "Government" },
            new CapexCategory { Id = 2, Name = "Employment" },
            new CapexCategory { Id = 3, Name = "Investment" },
            new CapexCategory { Id = 4, Name = "Food and Drinks" },
            new CapexCategory { Id = 5, Name = "Entertainment" },
            new CapexCategory { Id = 6, Name = "Education" },
            new CapexCategory { Id = 7, Name = "Social Expenses" },
            new CapexCategory { Id = 8, Name = "Medicines" },
            new CapexCategory { Id = 9, Name = "Assets" },
            new CapexCategory { Id = 10, Name = "Public Transport" },
            new CapexCategory { Id = 11, Name = "Housing" }
        );

        modelBuilder.Entity<CapexStatus>().HasData(
            new CapexStatus { Id = 1, Name = "Planning", Color = "blue" },
            new CapexStatus { Id = 2, Name = "Completed", Color = "green" },
            new CapexStatus { Id = 3, Name = "Canceled", Color = "red" }
        );
        
        // OPEX module seed data
        modelBuilder.Entity<OpexCategory>().HasData(
            new OpexCategory { Id = 1, Name = "Government" },
            new OpexCategory { Id = 2, Name = "Employment" },
            new OpexCategory { Id = 3, Name = "Investment" },
            new OpexCategory { Id = 4, Name = "Software" },
            new OpexCategory { Id = 5, Name = "Entertainment" },
            new OpexCategory { Id = 6, Name = "Vehicles" },
            new OpexCategory { Id = 7, Name = "Facilities" }
        );

        modelBuilder.Entity<OpexStatus>().HasData(
            new OpexStatus { Id = 1, Name = "Planning", Color = "blue" },
            new OpexStatus { Id = 2, Name = "Active", Color = "green" },
            new OpexStatus { Id = 3, Name = "Paused", Color = "gold" },
            new OpexStatus { Id = 4, Name = "Closed", Color = "red" }
        );
        
        // INVENTORY module seed data
        modelBuilder.Entity<InvVendorStatus>().HasData(
            new InvVendorStatus { Id = 1, Name = "Planning", Color = "blue" },
            new InvVendorStatus { Id = 2, Name = "Active", Color = "green" },
            new InvVendorStatus { Id = 3, Name = "Closed", Color = "red" }
        );
        
        modelBuilder.Entity<InvItemCategory>().HasData(
            new InvItemCategory { Id = 1, Name = "Bath Amenities" },
            new InvItemCategory { Id = 2, Name = "Beauty Products" },
            new InvItemCategory { Id = 3, Name = "Cleaning Products" },
            new InvItemCategory { Id = 4, Name = "Medicines" },
            new InvItemCategory { Id = 5, Name = "Complements" },
            new InvItemCategory { Id = 6, Name = "Raw Ingredients" },
            new InvItemCategory { Id = 7, Name = "Food Condiments" },
            new InvItemCategory { Id = 8, Name = "Drinks" },
            new InvItemCategory { Id = 9, Name = "Meal Ready to Eat" },
            new InvItemCategory { Id = 10, Name = "Office Supplies" },
            new InvItemCategory { Id = 11, Name = "Other Consumibles" }
        );

        modelBuilder.Entity<InvItemStatus>().HasData(
            new InvItemStatus { Id = 1, Name = "Active", Color = "green" },
            new InvItemStatus { Id = 2, Name = "Deprecated", Color = "red" },
            new InvItemStatus { Id = 3, Name = "Replaced", Color = "blue" }
        );
        
        modelBuilder.Entity<InvOrderStatus>().HasData(
            new InvOrderStatus { Id = 1, Name = "Planning", Color = "blue" },
            new InvOrderStatus { Id = 2, Name = "In Progress", Color = "gold" },
            new InvOrderStatus { Id = 3, Name = "Completed", Color = "green" },
            new InvOrderStatus { Id = 4, Name = "Canceled", Color = "red" }
        );
        
        // ASSET module seed data
        modelBuilder.Entity<AssetCategory>().HasData(
            new AssetCategory { Id = 1, Name = "Computers" },
            new AssetCategory { Id = 2, Name = "Other devices" },
            new AssetCategory { Id = 3, Name = "Appliances" },
            new AssetCategory { Id = 4, Name = "Furniture" },
            new AssetCategory { Id = 5, Name = "Decoration" },
            new AssetCategory { Id = 6, Name = "Commodities" },
            new AssetCategory { Id = 7, Name = "Vehicles" }
        );

        modelBuilder.Entity<AssetStatus>().HasData(
            new AssetStatus { Id = 1, Name = "Planning", Color = "blue" },
            new AssetStatus { Id = 2, Name = "Active", Color = "green" },
            new AssetStatus { Id = 3, Name = "Unavailable", Color = "gold" },
            new AssetStatus { Id = 4, Name = "Retired", Color = "red" }
        );
        
        // TRAVEL module seed data
        modelBuilder.Entity<TravelCategory>().HasData(
            new TravelCategory { Id = 1, Name = "Local" },
            new TravelCategory { Id = 2, Name = "Regional" },
            new TravelCategory { Id = 3, Name = "National" },
            new TravelCategory { Id = 4, Name = "Schengen" },
            new TravelCategory { Id = 5, Name = "Non-Schengen" }
        );

        modelBuilder.Entity<TravelStatus>().HasData(
            new TravelStatus { Id = 1, Name = "Planning", Color = "blue" },
            new TravelStatus { Id = 2, Name = "Active", Color = "gold" },
            new TravelStatus { Id = 3, Name = "Completed", Color = "green" },
            new TravelStatus { Id = 4, Name = "Canceled", Color = "red" }
        );
        
        modelBuilder.Entity<TravelCostCenter>().HasData(
            new TravelCostCenter { Id = 1, Name = "Armali" },
            new TravelCostCenter { Id = 2, Name = "Common Fund" },
            new TravelCostCenter { Id = 3, Name = "AMI3" },
            new TravelCostCenter { Id = 4, Name = "Other CC" }
        );
        
        modelBuilder.Entity<TravelSubEntityCategory>().HasData(
            new TravelSubEntityCategory { Id = 1, Name = "Hotels" },
            new TravelSubEntityCategory { Id = 2, Name = "Other Lodging" },
            new TravelSubEntityCategory { Id = 3, Name = "Airplane" },
            new TravelSubEntityCategory { Id = 4, Name = "Train" },
            new TravelSubEntityCategory { Id = 5, Name = "Car Rental" },
            new TravelSubEntityCategory { Id = 6, Name = "Metro" },
            new TravelSubEntityCategory { Id = 7, Name = "Taxi" },
            new TravelSubEntityCategory { Id = 8, Name = "Other Transport" },
            new TravelSubEntityCategory { Id = 9, Name = "Food and Drinks" },
            new TravelSubEntityCategory { Id = 10, Name = "Entertainment" },
            new TravelSubEntityCategory { Id = 11, Name = "Souvenirs" },
            new TravelSubEntityCategory { Id = 12, Name = "Visa or eTA" },
            new TravelSubEntityCategory { Id = 13, Name = "Other Expenses" }
        );
        
        // PROJECT module seed data
        modelBuilder.Entity<ProjectSubEntityCategory>().HasData(
            new ProjectSubEntityCategory { Id = 1, Name = "Project Documentation" },
            new ProjectSubEntityCategory { Id = 2, Name = "Risk Analysis" },
            new ProjectSubEntityCategory { Id = 3, Name = "Contract" },
            new ProjectSubEntityCategory { Id = 4, Name = "Invoice" },
            new ProjectSubEntityCategory { Id = 5, Name = "Other Documentation" },
            new ProjectSubEntityCategory { Id = 6, Name = "Project Output" }
        );

        modelBuilder.Entity<ProjectStatus>().HasData(
            new ProjectStatus { Id = 1, Name = "Planning", Color = "blue" },
            new ProjectStatus { Id = 2, Name = "Active", Color = "gold" },
            new ProjectStatus { Id = 3, Name = "Paused", Color = "gray" },
            new ProjectStatus { Id = 4, Name = "Completed", Color = "green" },
            new ProjectStatus { Id = 5, Name = "Closed", Color = "red" }
        );
        
        modelBuilder.Entity<ProjectProgram>().HasData(
            new ProjectProgram { Id = 1, Name = "DIGI" },
            new ProjectProgram { Id = 2, Name = "ENTR" },
            new ProjectProgram { Id = 3, Name = "EXPL" },
            new ProjectProgram { Id = 4, Name = "HOME" },
            new ProjectProgram { Id = 5, Name = "LOGI" },
            new ProjectProgram { Id = 6, Name = "PLAT" },
            new ProjectProgram { Id = 7, Name = "SOCI" }
        );
        
        modelBuilder.Entity<ProjectAxis>().HasData(
            new ProjectAxis { Id = 1, Name = "DEVL", ProgramId = 1},
            new ProjectAxis { Id = 2, Name = "MODS", ProgramId = 1},
            new ProjectAxis { Id = 3, Name = "MUSI", ProgramId = 1},
            new ProjectAxis { Id = 4, Name = "TVMV", ProgramId = 1},
            new ProjectAxis { Id = 5, Name = "VGME", ProgramId = 1},
            new ProjectAxis { Id = 6, Name = "ARTS", ProgramId = 2},
            new ProjectAxis { Id = 7, Name = "BOOK", ProgramId = 2},
            new ProjectAxis { Id = 8, Name = "BRGM", ProgramId = 2},
            new ProjectAxis { Id = 9, Name = "MISC", ProgramId = 2},
            new ProjectAxis { Id = 10, Name = "HOLI", ProgramId = 3},
            new ProjectAxis { Id = 11, Name = "VEHI", ProgramId = 3},
            new ProjectAxis { Id = 12, Name = "WMAP", ProgramId = 3},
            new ProjectAxis { Id = 13, Name = "WORK", ProgramId = 3},
            new ProjectAxis { Id = 14, Name = "EQPM", ProgramId = 4},
            new ProjectAxis { Id = 15, Name = "FUNC", ProgramId = 4},
            new ProjectAxis { Id = 16, Name = "FURN", ProgramId = 4},
            new ProjectAxis { Id = 17, Name = "STRU", ProgramId = 4},
            new ProjectAxis { Id = 18, Name = "DOCU", ProgramId = 5},
            new ProjectAxis { Id = 19, Name = "GOVM", ProgramId = 5},
            new ProjectAxis { Id = 20, Name = "OPEX", ProgramId = 5},
            new ProjectAxis { Id = 21, Name = "PROC", ProgramId = 5},
            new ProjectAxis { Id = 22, Name = "AGEN", ProgramId = 6},
            new ProjectAxis { Id = 23, Name = "AZUR", ProgramId = 6},
            new ProjectAxis { Id = 24, Name = "CTRL", ProgramId = 6},
            new ProjectAxis { Id = 25, Name = "EXTR", ProgramId = 6},
            new ProjectAxis { Id = 26, Name = "MGMT", ProgramId = 6},
            new ProjectAxis { Id = 27, Name = "NETW", ProgramId = 6},
            new ProjectAxis { Id = 28, Name = "FIRE", ProgramId = 7},
            new ProjectAxis { Id = 29, Name = "GRUP", ProgramId = 7},
            new ProjectAxis { Id = 30, Name = "SIPL", ProgramId = 7}
        );
        
        modelBuilder.Entity<ProjectRiskCategory>().HasData(
            new ProjectRiskCategory { Id = 1, Name = "Technical" },
            new ProjectRiskCategory { Id = 2, Name = "Financial" },
            new ProjectRiskCategory { Id = 3, Name = "Schedule" },
            new ProjectRiskCategory { Id = 4, Name = "Scope" },
            new ProjectRiskCategory { Id = 5, Name = "Resource" },
            new ProjectRiskCategory { Id = 6, Name = "External" },
            new ProjectRiskCategory { Id = 7, Name = "Legal" },
            new ProjectRiskCategory { Id = 8, Name = "Operational" }
        );
        
        // ARCHIVE module seed data
        modelBuilder.Entity<ArchiveCategory>().HasData(
            new ArchiveCategory { Id = 1, Name = "Government" },
            new ArchiveCategory { Id = 2, Name = "Employment" },
            new ArchiveCategory { Id = 3, Name = "Banking" },
            new ArchiveCategory { Id = 4, Name = "Other Business" },
            new ArchiveCategory { Id = 5, Name = "Entertainment" },
            new ArchiveCategory { Id = 6, Name = "Social Affairs" },
            new ArchiveCategory { Id = 7, Name = "Medical" },
            new ArchiveCategory { Id = 8, Name = "Assets" }
        );

        modelBuilder.Entity<ArchiveStatus>().HasData(
            new ArchiveStatus { Id = 1, Name = "Pending", Color = "gold" },
            new ArchiveStatus { Id = 2, Name = "Active", Color = "green" },
            new ArchiveStatus { Id = 3, Name = "Deprecated", Color = "red" }
        );
        
        // MAINT module seed data
        modelBuilder.Entity<MaintCategory>().HasData(
            new MaintCategory { Id = 1, Name = "Platform" },
            new MaintCategory { Id = 2, Name = "Appliances" },
            new MaintCategory { Id = 3, Name = "Furniture" },
            new MaintCategory { Id = 4, Name = "Decoration" },
            new MaintCategory { Id = 5, Name = "Vehicles" },
            new MaintCategory { Id = 6, Name = "Computers" },
            new MaintCategory { Id = 7, Name = "Assets" }
        );

        modelBuilder.Entity<MaintStatus>().HasData(
            new MaintStatus { Id = 1, Name = "Created", Color = "blue" },
            new MaintStatus { Id = 2, Name = "Active", Color = "gold" },
            new MaintStatus { Id = 3, Name = "Completed", Color = "green" },
            new MaintStatus { Id = 4, Name = "Canceled", Color = "red" }
        );
        
        // FIREBIRD module seed data
        modelBuilder.Entity<FirebirdCategory>().HasData(
            new FirebirdCategory { Id = 1, Name = "Cat A" },
            new FirebirdCategory { Id = 2, Name = "Cat B" },
            new FirebirdCategory { Id = 3, Name = "Cat C" },
            new FirebirdCategory { Id = 4, Name = "Cat D" },
            new FirebirdCategory { Id = 5, Name = "Cat E" },
            new FirebirdCategory { Id = 6, Name = "Cat F" }
        );

        modelBuilder.Entity<FirebirdStatus>().HasData(
            new FirebirdStatus { Id = 1, Name = "Unknown", Color = "blue" },
            new FirebirdStatus { Id = 2, Name = "Available", Color = "green" },
            new FirebirdStatus { Id = 3, Name = "Unavailable", Color = "red" },
            new FirebirdStatus { Id = 4, Name = "Blocked", Color = "gray" }
        );
        
        // CLOTHES module seed data
        modelBuilder.Entity<ClothesCategory>().HasData(
            new ClothesCategory { Id = 1, Name = "Short T-Shirt" },
            new ClothesCategory { Id = 2, Name = "Long T-Shirt" },
            new ClothesCategory { Id = 3, Name = "Short Shirt" },
            new ClothesCategory { Id = 4, Name = "Long Shirt" },
            new ClothesCategory { Id = 5, Name = "Short Polo" },
            new ClothesCategory { Id = 6, Name = "Long Polo" },
            new ClothesCategory { Id = 7, Name = "Short Trouser" },
            new ClothesCategory { Id = 8, Name = "Long Trouser" },
            new ClothesCategory { Id = 9, Name = "Jacket" },
            new ClothesCategory { Id = 10, Name = "Coat" },
            new ClothesCategory { Id = 11, Name = "Footwear" }
        );

        modelBuilder.Entity<ClothesStatus>().HasData(
            new ClothesStatus { Id = 1, Name = "Planning", Color = "blue" },
            new ClothesStatus { Id = 2, Name = "Active", Color = "green" },
            new ClothesStatus { Id = 3, Name = "Retired", Color = "red" }
        );
        
        modelBuilder.Entity<ClothesWashType>().HasData(
            new ClothesWashType { Id = 1, Name = "White Wash" },
            new ClothesWashType { Id = 2, Name = "Color Wash" },
            new ClothesWashType { Id = 3, Name = "Special Wash" },
            new ClothesWashType { Id = 4, Name = "Wash Alone" }
        );
        
        modelBuilder.Entity<ClothesColorStyle>().HasData(
            new ClothesColorStyle { Id = 1, Name = "Primary" },
            new ClothesColorStyle { Id = 2, Name = "Secondary" },
            new ClothesColorStyle { Id = 3, Name = "Details" }
        );
        
        modelBuilder.Entity<ClothesColor>().HasData(
            new ClothesColor { Id = 1, Name = "Maroon", Reference = "#800000" },
            new ClothesColor { Id = 2, Name = "Auburn", Reference = "#D22C21" },
            new ClothesColor { Id = 3, Name = "Coral", Reference = "#E51D2E" },
            new ClothesColor { Id = 4, Name = "Vermilion", Reference = "#E62E00" },
            new ClothesColor { Id = 5, Name = "Indian", Reference = "#FF8000" },
            new ClothesColor { Id = 6, Name = "Flame", Reference = "#F98F1D" },
            new ClothesColor { Id = 7, Name = "Amber", Reference = "#FFBF00" },
            new ClothesColor { Id = 8, Name = "Hansa", Reference = "#FFDF00" },
            new ClothesColor { Id = 9, Name = "Lime", Reference = "#D9E542" },
            new ClothesColor { Id = 10, Name = "Chartreuse", Reference = "#75B313" },
            new ClothesColor { Id = 11, Name = "Shamrock", Reference = "#009E60" },
            new ClothesColor { Id = 12, Name = "Viridian", Reference = "#007F5C" },
            new ClothesColor { Id = 13, Name = "Emerald", Reference = "#3FD8AA" },
            new ClothesColor { Id = 14, Name = "Verdigris", Reference = "#43B3AE" },
            new ClothesColor { Id = 15, Name = "Munsell", Reference = "#367588" },
            new ClothesColor { Id = 16, Name = "Sky", Reference = "#0CB7F2" },
            new ClothesColor { Id = 17, Name = "Cerulean", Reference = "#0087D1" },
            new ClothesColor { Id = 18, Name = "Indigo", Reference = "#0A3F7A" },
            new ClothesColor { Id = 19, Name = "Cobalt", Reference = "#333C87" },
            new ClothesColor { Id = 20, Name = "Violet", Reference = "#4C2882" },
            new ClothesColor { Id = 21, Name = "Iris", Reference = "#7F68A5" },
            new ClothesColor { Id = 22, Name = "Mauve", Reference = "#E0B0FF" },
            new ClothesColor { Id = 23, Name = "Orcein", Reference = "#C20073" },
            new ClothesColor { Id = 24, Name = "Salmon", Reference = "#EB6362" },
            new ClothesColor { Id = 25, Name = "Sandy", Reference = "#ECE2C6" },
            new ClothesColor { Id = 26, Name = "Sienna", Reference = "#C58A3E" },
            new ClothesColor { Id = 27, Name = "Cinnamon", Reference = "#8D4925" },
            new ClothesColor { Id = 28, Name = "Umber", Reference = "#955F20" },
            new ClothesColor { Id = 29, Name = "Chestnut", Reference = "#5D432C" },
            new ClothesColor { Id = 30, Name = "Sepia", Reference = "#663B2A" },
            new ClothesColor { Id = 31, Name = "Artemisia", Reference = "#E0E5FF" },
            new ClothesColor { Id = 32, Name = "Ash", Reference = "#CDCDCD" },
            new ClothesColor { Id = 33, Name = "Steel", Reference = "#8B8589" },
            new ClothesColor { Id = 34, Name = "Slate", Reference = "#5D6770" },
            new ClothesColor { Id = 35, Name = "Anthracite", Reference = "#383E42" },
            new ClothesColor { Id = 36, Name = "Cordovan", Reference = "#3B2A21" }
        );
        
        // MOOD module seed data
        modelBuilder.Entity<MoodCategory>().HasData(
            new MoodCategory { Id = 1, Name = "Happy", PrimaryColor = "green", SecondaryColor = "green" },
            new MoodCategory { Id = 2, Name = "  Excited", PrimaryColor = "green", SecondaryColor = "sky" },
            new MoodCategory { Id = 3, Name = "  Optimistic", PrimaryColor = "green", SecondaryColor = "azure" },
            new MoodCategory { Id = 4, Name = "  Grateful", PrimaryColor = "green", SecondaryColor = "pink" },
            new MoodCategory { Id = 5, Name = "  Vibing", PrimaryColor = "green", SecondaryColor = "viridian" },
            new MoodCategory { Id = 6, Name = "Playful", PrimaryColor = "sky", SecondaryColor = "sky" },
            new MoodCategory { Id = 7, Name = "  Flirty", PrimaryColor = "sky", SecondaryColor = "pink" },
            new MoodCategory { Id = 8, Name = "  Cheeky", PrimaryColor = "sky", SecondaryColor = "azure" },
            new MoodCategory { Id = 9, Name = "  Energetic", PrimaryColor = "sky", SecondaryColor = "spring" },
            new MoodCategory { Id = 10, Name = "  Funny", PrimaryColor = "sky", SecondaryColor = "green" },
            new MoodCategory { Id = 11, Name = "Focused", PrimaryColor = "teal", SecondaryColor = "teal" },
            new MoodCategory { Id = 12, Name = "  Productive", PrimaryColor = "teal", SecondaryColor = "azure" },
            new MoodCategory { Id = 13, Name = "  Absorbed", PrimaryColor = "teal", SecondaryColor = "viridian" },
            new MoodCategory { Id = 14, Name = "  Flowing", PrimaryColor = "teal", SecondaryColor = "spring" },
            new MoodCategory { Id = 15, Name = "  Withdrawn", PrimaryColor = "teal", SecondaryColor = "cobalt" },
            new MoodCategory { Id = 16, Name = "Love", PrimaryColor = "pink", SecondaryColor = "pink" },
            new MoodCategory { Id = 17, Name = "  Affectionate", PrimaryColor = "pink", SecondaryColor = "sky" },
            new MoodCategory { Id = 18, Name = "  Caring", PrimaryColor = "pink", SecondaryColor = "teal" },
            new MoodCategory { Id = 19, Name = "  Safe", PrimaryColor = "pink", SecondaryColor = "viridian" },
            new MoodCategory { Id = 20, Name = "  Connected", PrimaryColor = "pink", SecondaryColor = "green" },
            new MoodCategory { Id = 21, Name = "Confident", PrimaryColor = "azure", SecondaryColor = "azure" },
            new MoodCategory { Id = 22, Name = "  Empowered", PrimaryColor = "azure", SecondaryColor = "green" },
            new MoodCategory { Id = 23, Name = "  Proud", PrimaryColor = "azure", SecondaryColor = "pink" },
            new MoodCategory { Id = 24, Name = "  Determined", PrimaryColor = "azure", SecondaryColor = "teal" },
            new MoodCategory { Id = 25, Name = "  Bold", PrimaryColor = "azure", SecondaryColor = "spring" },
            new MoodCategory { Id = 26, Name = "Inspired", PrimaryColor = "spring", SecondaryColor = "spring" },
            new MoodCategory { Id = 27, Name = "  Curious", PrimaryColor = "spring", SecondaryColor = "teal" },
            new MoodCategory { Id = 28, Name = "  Daring", PrimaryColor = "spring", SecondaryColor = "azure" },
            new MoodCategory { Id = 29, Name = "  Amazed", PrimaryColor = "spring", SecondaryColor = "green" },
            new MoodCategory { Id = 30, Name = "  Creative", PrimaryColor = "spring", SecondaryColor = "sky" },
            new MoodCategory { Id = 31, Name = "Peaceful", PrimaryColor = "viridian", SecondaryColor = "viridian" },
            new MoodCategory { Id = 32, Name = "  Relaxed", PrimaryColor = "viridian", SecondaryColor = "azure" },
            new MoodCategory { Id = 33, Name = "  Satisfied", PrimaryColor = "viridian", SecondaryColor = "green" },
            new MoodCategory { Id = 34, Name = "  Relieved", PrimaryColor = "viridian", SecondaryColor = "teal" },
            new MoodCategory { Id = 35, Name = "  Serene", PrimaryColor = "viridian", SecondaryColor = "spring" },
            new MoodCategory { Id = 36, Name = "Lazy", PrimaryColor = "gray", SecondaryColor = "gray" },
            new MoodCategory { Id = 37, Name = "  Self-Care", PrimaryColor = "gray", SecondaryColor = "azure" },
            new MoodCategory { Id = 38, Name = "  Healing", PrimaryColor = "gray", SecondaryColor = "green" },
            new MoodCategory { Id = 39, Name = "  Indoor", PrimaryColor = "gray", SecondaryColor = "teal" },
            new MoodCategory { Id = 40, Name = "  Indifferent", PrimaryColor = "gray", SecondaryColor = "spring" },
            new MoodCategory { Id = 41, Name = "Fine", PrimaryColor = "white", SecondaryColor = "white" },
            new MoodCategory { Id = 42, Name = "Insecure", PrimaryColor = "orange", SecondaryColor = "orange" },
            new MoodCategory { Id = 43, Name = "  Ashamed", PrimaryColor = "orange", SecondaryColor = "red" },
            new MoodCategory { Id = 44, Name = "  Worthless", PrimaryColor = "orange", SecondaryColor = "cobalt" },
            new MoodCategory { Id = 45, Name = "  Doubtful", PrimaryColor = "orange", SecondaryColor = "brown" },
            new MoodCategory { Id = 46, Name = "  Paranoid", PrimaryColor = "orange", SecondaryColor = "indigo" },
            new MoodCategory { Id = 47, Name = "Angry", PrimaryColor = "red", SecondaryColor = "red" },
            new MoodCategory { Id = 48, Name = "  Frustrated", PrimaryColor = "red", SecondaryColor = "brown" },
            new MoodCategory { Id = 49, Name = "  Bitter", PrimaryColor = "red", SecondaryColor = "violet" },
            new MoodCategory { Id = 50, Name = "  Resentful", PrimaryColor = "red", SecondaryColor = "cobalt" },
            new MoodCategory { Id = 51, Name = "  Furious", PrimaryColor = "red", SecondaryColor = "yellow" },
            new MoodCategory { Id = 52, Name = "Depleted", PrimaryColor = "purple", SecondaryColor = "purple" },
            new MoodCategory { Id = 53, Name = "  Tired", PrimaryColor = "purple", SecondaryColor = "viridian" },
            new MoodCategory { Id = 54, Name = "  Burnout", PrimaryColor = "purple", SecondaryColor = "violet" },
            new MoodCategory { Id = 55, Name = "  Apathetic", PrimaryColor = "purple", SecondaryColor = "indigo" },
            new MoodCategory { Id = 56, Name = "  Numb", PrimaryColor = "purple", SecondaryColor = "white" },
            new MoodCategory { Id = 57, Name = "Uncomfortable", PrimaryColor = "yellow", SecondaryColor = "yellow" },
            new MoodCategory { Id = 58, Name = "  Disgusted", PrimaryColor = "yellow", SecondaryColor = "red" },
            new MoodCategory { Id = 59, Name = "  Jealous", PrimaryColor = "yellow", SecondaryColor = "pink" },
            new MoodCategory { Id = 60, Name = "  Embarrased", PrimaryColor = "yellow", SecondaryColor = "orange" },
            new MoodCategory { Id = 61, Name = "  Awkward", PrimaryColor = "yellow", SecondaryColor = "brown" },
            new MoodCategory { Id = 62, Name = "Tense", PrimaryColor = "violet", SecondaryColor = "violet" },
            new MoodCategory { Id = 63, Name = "  Anxious", PrimaryColor = "violet", SecondaryColor = "yellow" },
            new MoodCategory { Id = 64, Name = "  Overwhelmed", PrimaryColor = "violet", SecondaryColor = "red" },
            new MoodCategory { Id = 65, Name = "  Restless", PrimaryColor = "violet", SecondaryColor = "purple" },
            new MoodCategory { Id = 66, Name = "  Wary", PrimaryColor = "violet", SecondaryColor = "brown" },
            new MoodCategory { Id = 67, Name = "Confused", PrimaryColor = "brown", SecondaryColor = "brown" },
            new MoodCategory { Id = 68, Name = "  Indecisive", PrimaryColor = "brown", SecondaryColor = "orange" },
            new MoodCategory { Id = 69, Name = "  Lost", PrimaryColor = "brown", SecondaryColor = "indigo" },
            new MoodCategory { Id = 70, Name = "  Surprised", PrimaryColor = "brown", SecondaryColor = "yellow" },
            new MoodCategory { Id = 71, Name = "  Betrayed", PrimaryColor = "brown", SecondaryColor = "red" },
            new MoodCategory { Id = 72, Name = "Sad", PrimaryColor = "cobalt", SecondaryColor = "cobalt" },
            new MoodCategory { Id = 73, Name = "  Disappointed", PrimaryColor = "cobalt", SecondaryColor = "brown" },
            new MoodCategory { Id = 74, Name = "  Lonely", PrimaryColor = "cobalt", SecondaryColor = "viridian" },
            new MoodCategory { Id = 75, Name = "  Heartbroken", PrimaryColor = "cobalt", SecondaryColor = "pink" },
            new MoodCategory { Id = 76, Name = "  Nostalgic", PrimaryColor = "cobalt", SecondaryColor = "spring" },
            new MoodCategory { Id = 77, Name = "Scared", PrimaryColor = "indigo", SecondaryColor = "indigo" },
            new MoodCategory { Id = 78, Name = "  Startled", PrimaryColor = "indigo", SecondaryColor = "brown" },
            new MoodCategory { Id = 79, Name = "  Worried", PrimaryColor = "indigo", SecondaryColor = "violet" },
            new MoodCategory { Id = 80, Name = "  Suspicious", PrimaryColor = "indigo", SecondaryColor = "orange" },
            new MoodCategory { Id = 81, Name = "  Pesimistic", PrimaryColor = "indigo", SecondaryColor = "cobalt" }
        );
    }
}