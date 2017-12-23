node {
	stage 'Checkout'
		checkout scm

	stage 'Build'
		bat 'nuget restore SolutionName.sln'
		bat "\"${tool 'MSBuild'}\" NosSharp.sln /p:Configuration=Debug /p:Platform=\"Any CPU\"

	stage 'Archive'
		archive 'ProjectName/bin/Debug/**'

}