using Microsoft.EntityFrameworkCore.Migrations;

namespace CMSCore.Migrations
{
    public partial class chatnew2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_ChatCUser_ChatId",
                table: "ChatUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatCUser_ChatId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatCUser",
                table: "ChatCUser");

            migrationBuilder.RenameTable(
                name: "ChatCUser",
                newName: "Chats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chats",
                table: "Chats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Chats_ChatId",
                table: "ChatUsers",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_Chats_ChatId",
                table: "ChatUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chats",
                table: "Chats");

            migrationBuilder.RenameTable(
                name: "Chats",
                newName: "ChatCUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatCUser",
                table: "ChatCUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_ChatCUser_ChatId",
                table: "ChatUsers",
                column: "ChatId",
                principalTable: "ChatCUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatCUser_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "ChatCUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
