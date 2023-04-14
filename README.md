# Statictizer

Statictizer is a multi-tenant host for static websites. It helps to consolidate resources when
a large number of static websites need to be  hosted for an organization.

There is currently no built-in limit to the number of sites you can have, nor the size of those sites.

The intended architecture is to use a host-aware CDN in front of the server service.

## Bits

There are a few bits.

**api**  
The API used to manage user interactions with the storage system.

**serve**  
The service used to actually deliver the content form the storage system.

**cli**  
The console app to upload sites.

**shared**  
Models and helpers.

**ui**  
The user interface.

## Local Development

The only requirement outside of dotnet 7 (as of this writing) is a mongo server. I use a locally installed mongo.

To make multi-tenancy easier to test I've provided `*.sites.belkonar.com`. This is a wildcard route that routes
to `127.0.0.1`.

You can use that, or just `localhost` to test the server.
