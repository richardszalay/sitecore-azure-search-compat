Solves two major limitations with Sitecore's support for Azure Search:

1. Facetable/Sortable/Filterable field values larger than 32766 bytes are intelligently truncated rather than omitted (XP 9.1) or failing the entire document (XP 9.0)
2. Document batches are dynamically split into smaller batches if they would breach the 16mb Azure request limit, removing the need to reduce `maxDocuments` from its default value of 1000.

NOTE: This patch _replaces_ the need for [Hotfix 176528](https://github.com/SitecoreSupport/Sitecore.Support.176528). If installed, the hotfix should be removed as it will continue to omit all fields larger than 32766 bytes (even if they would not otherwise generate errors in Azure).

## Installation

This patch is currently distributed as source, and includes unit tests.

The `Community.Sitecore.ContentSearch.Azure.config` config patch also needs to be included, but requires modification:

The previously configured `contentSearch/searchService` (but **NOT** `index/searchService`) needs to be reconfigured as 
`<innerSearchService>`. The default is provided as an example in `Community.Sitecore.ContentSearch.Azure.config`; if that's 
fine, just uncomment it. If you have a hotfix applied it may need to be customised.

## Related Errors

If you're seeing any of the following errors in logs, this project should help.

### Facetable/Sortable/Filterable field values larger than 32766 bytes

> Exception: Sitecore.ContentSearch.Azure.Http.Exceptions.PostFailedForSomeDocumentsException
> Message: Partial success for insert or update. Some documents succeeded, but at least one failed.
> 
> Field 'content_s' contains a term that is too large to process. The max length for UTF-8 encoded terms is 32766 bytes. The most likely cause of this error is that filtering, sorting, and/or faceting are enabled on this field, which causes the entire field value to be indexed as a single term. Please avoid the use of these options for large

### Batch request too large

> Sitecore.ContentSearch.Azure.Http.Exceptions.RequestEntityTooLargeException: Request size exceeded Azure Search Service limits
