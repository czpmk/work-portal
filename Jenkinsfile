pipeline {
    agent any
    stages {
        stage('Restore packages'){
           steps{
               sh 'dotnet restore WorkPortalApi.sln'
            }
         }
        stage('Clean'){
           steps{
               sh 'dotnet clean WorkPortalApi.sln --configuration Debug'
            }
         }
        stage('Build'){
           steps{
               sh 'dotnet build WorkPortalApi.sln --configuration Debug --no-restore'
            }
         }
        stage('Publish'){
             steps{
               sh 'dotnet publish WorkPortalAPI.csproj --configuration Debug --no-restore'
             }
        }
        stage('Deploy'){
             steps{
                sh 'rm WorkPortalAPI/bin/Debug/net5.0/publish/appsettings.json'
                sh 'cp -r -u WorkPortalAPI/bin/Debug/net5.0/publish/ /var/www/publish/'
                sh 'service workportal restart'
             }
        }
    }
}