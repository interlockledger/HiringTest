# HiringTest
A project template for testing dev/interns applicants

## How to Build

1. Fork this repo and clone to your machine, also clone and checkout repos 
https://github.com/interlockledger/peer2peer, https://github.com/interlockledger/il2tags and https://github.com/interlockledger/ilint
2. Set an environment variable INTERLOCK_LOCAL_NUGETS_FOLDER, persisted, to a path your user can write when building the above repos
3. Add the path chosen in 2 to the nuget sources
4. Build repos ilint, iltags, peer2peer in that order so that packages published are used by the next build
5. Lastly build this repo

## How to Test

See server.cmd and client.cmd to start both instances in that order (in Windows just use the .cmd files, in Linux you can write similar scripts)

You can then try the 'w' command to see if things are working (currently it should NOT)

## Tasks

1. Implement a new command 's' to send a file from the client to the server, storing it at some temporary folder
2. Implement a new command 'l' to list files present in the temporary folder in the server

### Rules

You should abide to the formatting/code conventions already in the sources (.editorconfig is used to help with that if you are using Visual Studio)
If using Visual Studio, use the latest 2019 release, supporting C# 8.0.
.NET Core must be version 2.2 (do not use 3.x for now)

Follow the architecture of defining Data classes for the commands/responses based on AbstractData, and serialize their Payload wrapper as the message body in the Peer2Peer channel

For understanding the kind of core types of tagged values and the ILTagxxx classes to help with them, look at the https://github.com/interlockledger/specification repo and then the implementation in IL2Tags, specially the stream extension methods for encoding/decoding the core types.

Work on your fork and when satisfied do a pull request for us to evaluate.
While working on it, you can create issues here with questions if needed.
We expect for these tasks to be fully understood and worked through in less than a week using part-time.
