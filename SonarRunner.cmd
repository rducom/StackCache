@echo off

IF "%APPVEYOR_PULL_REQUEST_NUMBER%"=="" (
MSBuild.SonarQube.Runner.exe begin /k:StackCache /n:StackCache /v:1.0 ^
	/d:sonar.host.url=%SonarServer% ^
	/d:sonar.login=%SonarUser% ^
	/d:sonar.password=%SonarPassword% ^
	/d:sonar.github.repository=%APPVEYOR_REPO_NAME% ^
	/d:sonar.github.login=StackCacheOps ^
	/d:sonar.github.oauth=753dc16f860f7b1581cdd8b9b9b77596270df73e 
) ELSE (
MSBuild.SonarQube.Runner.exe begin /k:StackCache /n:StackCache /v:1.0 ^
	/d:sonar.host.url=%SonarServer% ^
	/d:sonar.login=%SonarUser% ^
	/d:sonar.password=%SonarPassword% ^
	/d:sonar.analysis.mode=preview ^
	/d:sonar.github.pullRequest=%APPVEYOR_PULL_REQUEST_NUMBER% ^
	/d:sonar.github.repository=%APPVEYOR_REPO_NAME% ^
	/d:sonar.github.login=StackCacheOps ^
	/d:sonar.github.oauth=753dc16f860f7b1581cdd8b9b9b77596270df73e 
)