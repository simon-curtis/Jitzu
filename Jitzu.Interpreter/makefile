.PHONY: pub-win pub-linux install-win test

pub-win:
	dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -o bin/win-x64

pub-linux:
	dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -o bin/linux-x64

install-win: pub-win
	mkdir %USERPROFILE%/bin/Jiztu
	cp bin/Publish/jz.eze %USERPROFILE%/bin/Jiztu

test:
# 	dotnet run -- run "../site/docs/syntax/01_numbers.jz"
# 	dotnet run -- run "../site/docs/syntax/02_strings.jz"
# 	dotnet run -- run "../site/docs/syntax/04_dates.jz"
# 	dotnet run -- run "../site/docs/syntax/05_functions.jz"
# 	dotnet run -- run "../site/docs/syntax/06_logic_flow.jz"
# 	dotnet run -- run "../site/docs/syntax/07_dynamic.jz"
# 	dotnet run -- run "../site/docs/syntax/08_json.jz"
# 	dotnet run -- run "../site/docs/syntax/09_unions.jz"
# 	dotnet run -- run "../site/docs/syntax/10_imports.jz"
# 	dotnet run -- -d -t run "../site/docs/syntax/11_http_requests.jz"
# 	dotnet run -- run "../site/docs/syntax/12_files.jz"
# 	dotnet run -- run "../site/docs/syntax/13_traits.jz"
	dotnet run -- run "../site/docs/syntax/14_iter.jz"
