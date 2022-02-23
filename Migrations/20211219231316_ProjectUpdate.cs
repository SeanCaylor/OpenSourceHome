using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSourceHome.Migrations
{
    public partial class ProjectUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReplyLikes");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Posts",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(45) CHARACTER SET utf8mb4",
                oldMaxLength: 45);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Posts",
                type: "varchar(45) CHARACTER SET utf8mb4",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 150);

            migrationBuilder.CreateTable(
                name: "UserReplyLikes",
                columns: table => new
                {
                    UserReplyLikeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReplyId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReplyLikes", x => x.UserReplyLikeId);
                    table.ForeignKey(
                        name: "FK_UserReplyLikes_Replies_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "Replies",
                        principalColumn: "ReplyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReplyLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserReplyLikes_ReplyId",
                table: "UserReplyLikes",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReplyLikes_UserId",
                table: "UserReplyLikes",
                column: "UserId");
        }
    }
}
