using System;
using System.Collections.Generic;
using DealtHands.ModelsV2;
using Microsoft.EntityFrameworkCore;

namespace DealtHands.Data;

public partial class DealtHandsDbv2Context : DbContext
{
    public DealtHandsDbv2Context()
    {
    }

    public DealtHandsDbv2Context(DbContextOptions<DealtHandsDbv2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Card> Cards { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameChanger> GameChangers { get; set; }

    public virtual DbSet<GameRound> GameRounds { get; set; }

    public virtual DbSet<GameSession> GameSessions { get; set; }

    public virtual DbSet<Ugc> Ugcs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__Card__55FECDAE04C2E5A4");

            entity.ToTable("Card");

            entity.HasIndex(e => e.CardType, "idx_card_cardtype");

            entity.HasIndex(e => e.RoundType, "idx_card_roundtype");

            entity.Property(e => e.CardType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("RoundCard");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MonthlyAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.DifficultyLevel).HasDefaultValue((byte)0);

            // Requires* columns
            entity.Property(e => e.RequiresStudentLoans).IsRequired(false);
            entity.Property(e => e.RequiresCar).IsRequired(false);
            entity.Property(e => e.RequiresCarLoan).IsRequired(false);
            entity.Property(e => e.RequiresOwnsHome).IsRequired(false);
            entity.Property(e => e.RequiresApartment).IsRequired(false);
            entity.Property(e => e.RequiresRoommate).IsRequired(false);
            entity.Property(e => e.RequiresMarried).IsRequired(false);
            entity.Property(e => e.RequiresChildren).IsRequired(false);

            // Sets* columns
            entity.Property(e => e.SetsStudentLoans).IsRequired(false);
            entity.Property(e => e.SetsCar).IsRequired(false);
            entity.Property(e => e.SetsCarLoan).IsRequired(false);
            entity.Property(e => e.SetsOwnsHome).IsRequired(false);
            entity.Property(e => e.SetsApartment).IsRequired(false);
            entity.Property(e => e.SetsRoommate).IsRequired(false);
            entity.Property(e => e.SetsMarried).IsRequired(false);
            entity.Property(e => e.SetsChildren).IsRequired(false);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__Game__2AB897FDEF6AACC0");

            entity.ToTable("Game");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Mode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(80)
                .IsUnicode(false);
        });

        modelBuilder.Entity<GameChanger>(entity =>
        {
            entity.HasKey(e => e.GameChangerId).HasName("PK_GameChanger");

            entity.ToTable("GameChanger");

            entity.HasIndex(e => e.RoundType, "idx_gamechanger_roundtype");
            entity.HasIndex(e => e.DifficultyLevel, "idx_gamechanger_difficulty");

            entity.Property(e => e.Title)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DifficultyLevel).HasDefaultValue((byte)0);
            entity.Property(e => e.MonthlyAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Financial effect columns
            entity.Property(e => e.IncomeEffect).HasColumnType("decimal(10, 2)").IsRequired(false);
            entity.Property(e => e.IncomeEffectPercent).HasColumnType("decimal(5, 4)").IsRequired(false);
            entity.Property(e => e.ExpenseEffect).HasColumnType("decimal(10, 2)").IsRequired(false);

            // Selection weight (nullable; defaults to 5 in code when null)
            entity.Property(e => e.Weight).IsRequired(false);

            // Requires* columns
            entity.Property(e => e.RequiresApartment).IsRequired(false);
            entity.Property(e => e.RequiresCar).IsRequired(false);
            entity.Property(e => e.RequiresCarLoan).IsRequired(false);
            entity.Property(e => e.RequiresOwnsHome).IsRequired(false);
            entity.Property(e => e.RequiresStudentLoans).IsRequired(false);
            entity.Property(e => e.RequiresRoommate).IsRequired(false);
            entity.Property(e => e.RequiresMarried).IsRequired(false);
            entity.Property(e => e.RequiresChildren).IsRequired(false);
            entity.Property(e => e.RequiresJob).IsRequired(false);

            // Sets* columns
            entity.Property(e => e.SetsApartment).IsRequired(false);
            entity.Property(e => e.SetsCar).IsRequired(false);
            entity.Property(e => e.SetsCarLoan).IsRequired(false);
            entity.Property(e => e.SetsOwnsHome).IsRequired(false);
            entity.Property(e => e.SetsStudentLoans).IsRequired(false);
            entity.Property(e => e.SetsRoommate).IsRequired(false);
            entity.Property(e => e.SetsMarried).IsRequired(false);
            entity.Property(e => e.SetsChildren).IsRequired(false);
            entity.Property(e => e.SetsJob).IsRequired(false);
        });

        modelBuilder.Entity<GameRound>(entity =>
        {
            entity.HasKey(e => e.GameRoundId).HasName("PK__GameRoun__003F8F05D8CB7DE5");

            entity.ToTable("GameRound");

            entity.HasIndex(e => e.GameSessionId, "idx_gameround_session");

            entity.HasIndex(e => new { e.GameSessionId, e.RoundNumber }, "uq_gameround_session_roundnum").IsUnique();

            entity.Property(e => e.ClosedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OpenedAt).HasColumnType("datetime");
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("NotStarted");

            entity.HasOne(d => d.GameSession).WithMany(p => p.GameRounds)
                .HasForeignKey(d => d.GameSessionId)
                .HasConstraintName("fk_gameround_session");
        });

        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.GameSessionId).HasName("PK__GameSess__E588370D2F5B90D0");

            entity.ToTable("GameSession");

            entity.HasIndex(e => e.JoinCode, "UQ__GameSess__FF7C6BA0FE398DA1").IsUnique();

            entity.HasIndex(e => e.GameId, "idx_gamesession_game");

            entity.HasIndex(e => e.HostUserId, "idx_gamesession_host");

            entity.HasIndex(e => e.Status, "idx_gamesession_status");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentRoundNumber).HasDefaultValue((byte)1);
            entity.Property(e => e.EndedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JoinCode)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Difficulty)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PausedAt).HasColumnType("datetime");
            entity.Property(e => e.ResumedAt).HasColumnType("datetime");
            entity.Property(e => e.StartedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Waiting");

            entity.HasOne(d => d.Game).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gamesession_game");

            entity.HasOne(d => d.HostUser).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.HostUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gamesession_host");
        });

        modelBuilder.Entity<Ugc>(entity =>
        {
            entity.HasKey(e => e.UgcId).HasName("PK__UGC__DC0AF6F2A9F2DF4B");

            entity.ToTable("UGC");

            entity.HasIndex(e => e.CardId, "idx_ugc_card");

            entity.HasIndex(e => e.GameChangerId, "idx_ugc_gamechanger");

            entity.HasIndex(e => e.GameRoundId, "idx_ugc_round");

            entity.HasIndex(e => e.GameSessionId, "idx_ugc_session");

            entity.HasIndex(e => e.UserId, "idx_ugc_user");

            entity.HasIndex(e => new { e.UserId, e.GameRoundId, e.CardId }, "uq_ugc_user_round_card").IsUnique();

            entity.Property(e => e.UgcId).HasColumnName("UGCId");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RunningTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SubmittedAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SubmittedAt).HasColumnType("datetime");

            entity.Property(e => e.CardId).IsRequired(false);
            entity.Property(e => e.GameChangerId).IsRequired(false);

            entity.HasOne(d => d.Card).WithMany(p => p.Ugcs)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ugc_card");

            entity.HasOne(d => d.GameChanger).WithMany(p => p.Ugcs)
                .HasForeignKey(d => d.GameChangerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ugc_gamechanger");

            entity.HasOne(d => d.GameRound).WithMany(p => p.Ugcs)
                .HasForeignKey(d => d.GameRoundId)
                .HasConstraintName("fk_ugc_round");
            /* Removed the foreign key constraint from the DB on 3/30/26
            entity.HasOne(d => d.GameSession).WithMany(p => p.Ugcs)
                .HasForeignKey(d => d.GameSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ugc_session");
            */
            entity.HasOne(d => d.User).WithMany(p => p.Ugcs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ugc_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C7489493E");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E469253162").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D105341A37EEF6").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PasswordResetExpires)
                .HasColumnType("datetime");
            entity.Property(e => e.IsEducator).HasDefaultValue(false);

            // Player state flags
            entity.Property(e => e.HasStudentLoans).HasDefaultValue(false);
            entity.Property(e => e.HasCar).HasDefaultValue(false);
            entity.Property(e => e.HasCarLoan).HasDefaultValue(false);
            entity.Property(e => e.OwnsHome).HasDefaultValue(false);
            entity.Property(e => e.HasApartment).HasDefaultValue(false);
            entity.Property(e => e.HasRoommate).HasDefaultValue(false);
            entity.Property(e => e.IsMarried).HasDefaultValue(false);
            entity.Property(e => e.HasChildren).HasDefaultValue(false);
            entity.Property(e => e.HasJob).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
