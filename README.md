# Figaro.API
API definition and implementation for Figaro.



## Design Notes

- statistics enabled by default on everything.

- POSTing a container is not supported.

  ​

## Design TODO 

-  should encryption be something configured on a container create POST statement? Current thought is yes, upload a secret that is automatically put in key vault.
- ​



## Putting documents into a container

a PUT operation of an XML document or JSON payload will upload the request **and its header information** into the container. JSON payloads will convert into XML documents using the specified JSON parser. All header information will go into the document metadata. Users can add custom metadata information by specifying data in the header; developers wanting to specify custom namespaces and names can also include them in the header with the format of *http://namespace/#name* where the namespace and name are included but separated by the hashtag. The system will parse the two and use them in the document metadata. 