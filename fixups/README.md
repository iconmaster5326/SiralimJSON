# Manual Fixups

Not all data can be scraped. Some data needs to be added or changed manually. Here you may do so.

In each folder are a series of `.yaml` files, each one providing a patch to a single individual `.json` file in the exported output. The file names are irrelevant.

In each file, you must provide the `id` of the entity you're patching (or `name` for races). Each other field represents an override of the field of the same name. Note that `null` values override the corresponding field with `null`, so only provide the fields you want to overwrite!
