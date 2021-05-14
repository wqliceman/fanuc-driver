namespace fanuc
{
    public partial class Platform
    {
        public dynamic Connect()
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_allclibhndl3(
                    _machine.FocasEndpoint.IPAddress, 
                    _machine.FocasEndpoint.Port,
                    _machine.FocasEndpoint.ConnectionTimeout, 
                    out _handle);
            });

            return new
            {
                method = "cnc_allclibhndl3",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_allclibhndl3",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new
                {
                    cnc_allclibhndl3 = new
                        {
                            ipaddr = _machine.FocasEndpoint.IPAddress, 
                            port = _machine.FocasEndpoint.Port, 
                            timeout = _machine.FocasEndpoint.ConnectionTimeout
                        }
                },
                response = new {cnc_allclibhndl3 = new {FlibHndl = _handle}}
            };
        }
    }
}