# Group Chat

## Some commands
```git clone https://github.com/Hymeck/GroupChat``` - copy project.

In project folder:

```dotnet publish -c Release -o release src/GroupChat.sln``` - build project and place result in `release` folder.

## Make Docker container
```
docker build -t group-chat-image -f Dockerfile .
docker run -it group-chat-image
```


## Useful links (Linux)
- [Enable and disable root](https://linuxize.com/post/how-to-enable-and-disable-root-user-account-in-ubuntu/)
- [Docker permission denied](https://medium.com/@dhananjay4058/solving-docker-permission-denied-while-trying-to-connect-to-the-docker-daemon-socket-2e53cccffbaa)
- [Dockerize .NET application](https://docs.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=linux)