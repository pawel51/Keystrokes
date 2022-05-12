using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace KeystrokesData.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GraphData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Metric = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnnEdgeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    KnnGraphEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnnEdgeEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnnEdgeEntity_GraphData_KnnGraphEntityId",
                        column: x => x.KnnGraphEntityId,
                        principalTable: "GraphData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnnNodeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Category = table.Column<string>(type: "text", nullable: true),
                    KnnGraphEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnnNodeEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnnNodeEntity_GraphData_KnnGraphEntityId",
                        column: x => x.KnnGraphEntityId,
                        principalTable: "GraphData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingleProbe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AsciiSign = table.Column<string>(type: "text", nullable: true),
                    Flight = table.Column<double>(type: "double precision", nullable: false),
                    Dwell = table.Column<double>(type: "double precision", nullable: false),
                    TestSampleId = table.Column<int>(type: "integer", nullable: true),
                    TrainSampleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleProbe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleProbe_TestData_TestSampleId",
                        column: x => x.TestSampleId,
                        principalTable: "TestData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SingleProbe_TrainData_TrainSampleId",
                        column: x => x.TrainSampleId,
                        principalTable: "TrainData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnnEdgeEntity_KnnGraphEntityId",
                table: "KnnEdgeEntity",
                column: "KnnGraphEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_KnnNodeEntity_KnnGraphEntityId",
                table: "KnnNodeEntity",
                column: "KnnGraphEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleProbe_TestSampleId",
                table: "SingleProbe",
                column: "TestSampleId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleProbe_TrainSampleId",
                table: "SingleProbe",
                column: "TrainSampleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnnEdgeEntity");

            migrationBuilder.DropTable(
                name: "KnnNodeEntity");

            migrationBuilder.DropTable(
                name: "SingleProbe");

            migrationBuilder.DropTable(
                name: "GraphData");

            migrationBuilder.DropTable(
                name: "TestData");

            migrationBuilder.DropTable(
                name: "TrainData");
        }
    }
}
