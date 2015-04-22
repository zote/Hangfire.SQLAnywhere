Properties {
    $solution = "Hangfire.SQLAnywhere.sln"
}

Include "packages\Hangfire.Build.*\tools\psake-common.ps1"

Task Default -Depends Collect

Task Test -Depends Compile -Description "Run unit and integration tests." {
    Run-XunitTests "Hangfire.SQLAnywhere.Tests"
	Run-XunitTests "Hangfire.SQLAnywhere.Msmq.Tests"
}

Task Merge -Depends Test -Description "Run ILMerge /internalize to merge assemblies." {
    # Remove `*.pdb` file to be able to prepare NuGet symbol packages.
    Remove-Item ((Get-SrcOutputDir "Hangfire.SQLAnywhere") + "\Dapper.pdb")

    Merge-Assembly "Hangfire.SQLAnywhere" @("Dapper")
}

Task Collect -Depends Merge -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.SQLAnywhere" "Net45"
	Collect-Assembly "Hangfire.SQLAnywhere.Msmq" "Net45"

    Collect-Tool "src\Hangfire.SQLAnywhere\Install.v1.sql"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-BuildVersion

    Create-Archive "Hangfire-SQLAnywhere-$version"

    Create-Package "Hangfire.SQLAnywhere" $version
	Create-Package "Hangfire.SQLAnywhere.Msmq" $version
}