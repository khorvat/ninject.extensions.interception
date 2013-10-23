call tools\nuget\NuGet.exe pack "nuget/Ninject.Extensions.Interception.3.0.1-async02.nuspec" 
call tools\nuget\NuGet.exe pack "nuget-linfu/Ninject.Extensions.Interception.Linfu.3.0.1-async02.nuspec" 

move "Ninject.Extensions.Interception.3.0.1-async02.nupkg" \\buildserver\d$\Publish\NuGets\3rdParty\
move "Ninject.Extensions.Interception.Linfu.3.0.1-async02.nupkg" \\buildserver\d$\Publish\NuGets\3rdParty\
