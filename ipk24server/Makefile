all: clean
	dotnet build ./ipk24_proj2_src
	ln -s ./ipk24_proj2_src/bin/Debug/net8.0/ipk24_proj2_src ./ipk24chat-server
	
clean:
	dotnet clean ./ipk24_proj2_src
	rm -f ./ipk24chat-server
