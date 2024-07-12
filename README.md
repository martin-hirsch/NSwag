# Usage

## Controller

	docker run --rm --volume '$(PWD)/PathToOAS:/app/publish' nswag \
	'publish/my.oas.yaml' \
 	'My.Namespace' \
 	'controller'

  ## Client

	docker run --rm --volume '$(PWD)/PathToOAS:/app/publish' nswag \
	'publish/my.oas.yaml' \
 	'My.Namespace' \
 	'client'
