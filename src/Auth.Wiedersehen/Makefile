include ../../.env


migrate_configuration:
	dotnet ef migrations bundle -c ConfigurationDbContext --self-contained -r linux-x64 -o configuration_db --force
	./configuration_db --connection="${AW_CONFIGURATION_DB}"
	rm configuration_db

migrate_persistent_grant:
	dotnet ef migrations bundle -c PersistedGrantDbContext --self-contained -r linux-x64 -o persistent_grant_db --force
	./persistent_grant_db --connection="${AW_PERSISTENT_GRAND_DB}"
	rm persistent_grant_db

migrate_application:
	dotnet ef migrations bundle -c ApplicationDbContext --self-contained -r linux-x64 -o application_db --force
	./application_db --connection="${AW_APPLICATION_DB}"
	rm application_db

full_migrations:
	make migrate_configuration
	make migrate_persistent_grant
	make migrate_application
