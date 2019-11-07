# Access token

Access tokens are static secrets that can be used to access services. For security reasons it is not possible to store the access tokens directly in the json configuration file.

Instead secure access has been implemented.

## Keyvault storage

Azure Keyvault allows storing secrets and securely accessing them at runtime. In the configuration the keyvault url and secret name is needed:

``` json
{
    "accessToken": {
        "source": "https://mykeyvault.vault.azure.net",
        "secretName": "MyDevOpsBuildPAT"
    }
}
```

Once configured the Azure Function must be granted access to the keyvault. This can be achieved via the Managed Service Identity (MSI) that is enabled on the function as part of the ARM template deployment. (The MSI name will be the same as the azure function name).

In the Keyvault access policies the Azure Function will require `get,list` secret permissions to access the secret at runtime.