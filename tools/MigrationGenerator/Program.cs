using System;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.IO;
using AhuErp.Core.Migrations;

namespace AhuErp.Tools.MigrationGenerator
{
    /// <summary>
    /// Скаффолдер EF6-миграций. Запускается через <c>mono</c> на Linux или
    /// <c>dotnet</c> на Windows. Перед скаффолдингом принудительно применяет
    /// все уже существующие миграции к целевой БД (из <c>App.config</c>), так
    /// чтобы <see cref="MigrationScaffolder"/> видел только delta новой модели.
    /// Пишет <c>&lt;stamp&gt;_&lt;Name&gt;.cs / .Designer.cs / .resx</c>
    /// в указанную папку. Не входит в основной <c>.sln</c> — вспомогательный.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var migrationsDir = args.Length > 0
                ? args[0]
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations");
            var migrationName = args.Length > 1 ? args[1] : "InitialCreate";

            Directory.CreateDirectory(migrationsDir);

            var configuration = new Configuration();

            // Приводим БД к состоянию последнего закоммиченного снапшота —
            // без этого MigrationScaffolder сочтёт существующие миграции
            // «pending» и откажется генерировать дельту. Явно ограничиваем
            // апдейт последней уже написанной миграцией — иначе EF6 попытается
            // «доехать» до текущей модели через AutomaticMigrations.
            var migrator = new DbMigrator(configuration);
            string lastLocal = null;
            foreach (var id in migrator.GetLocalMigrations()) lastLocal = id;
            if (lastLocal != null) migrator.Update(lastLocal);

            var scaffolder = new MigrationScaffolder(configuration);
            var result = scaffolder.Scaffold(migrationName, ignoreChanges: false);

            var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var baseName = $"{stamp}_{migrationName}";

            File.WriteAllText(Path.Combine(migrationsDir, baseName + ".cs"), result.UserCode);
            File.WriteAllText(Path.Combine(migrationsDir, baseName + ".Designer.cs"), result.DesignerCode);

            using (var fs = File.Create(Path.Combine(migrationsDir, baseName + ".resx")))
            using (var writer = new System.Resources.ResXResourceWriter(fs))
            {
                foreach (var kvp in result.Resources)
                {
                    writer.AddResource(kvp.Key, kvp.Value);
                }
                writer.Generate();
            }

            Console.WriteLine($"Migration '{baseName}' written to {migrationsDir}");
            return 0;
        }
    }
}
