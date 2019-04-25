using Microsoft.EntityFrameworkCore.Migrations;

namespace IReckonUpload.Migrations
{
    public partial class AddRawDeliveryRange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_DeliveryRange_DelivedInId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "DelivedInId",
                table: "Products",
                newName: "DeliveredInId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_DelivedInId",
                table: "Products",
                newName: "IX_Products_DeliveredInId");

            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "DeliveryRange",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_DeliveryRange_DeliveredInId",
                table: "Products",
                column: "DeliveredInId",
                principalTable: "DeliveryRange",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_DeliveryRange_DeliveredInId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Raw",
                table: "DeliveryRange");

            migrationBuilder.RenameColumn(
                name: "DeliveredInId",
                table: "Products",
                newName: "DelivedInId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_DeliveredInId",
                table: "Products",
                newName: "IX_Products_DelivedInId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_DeliveryRange_DelivedInId",
                table: "Products",
                column: "DelivedInId",
                principalTable: "DeliveryRange",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
