# apod-dl

A simple [NASA Astronomy Picture of the Day](https://apod.nasa.gov) downloader.

You specify a date and the number of images you want, and this will download that many images into the directory you specified. If the image for a given day has already been downloaded it is skipped, and if there is no image for a given day then it goes back one extra day to make up for it.

## Usage

`apod-dl` accepts the following command line arguments:

```
  -d, --date        (Default: today) First date to retrieve.

  -c, --count       (Default: 1) The total number of images to retrieve and retain.

  -o, --outdir      Required. The directory to download the images to.

  -f, --fillsize    Resize the image to fill these dimensions (eg. 1920x1080)

  --quiet           (Default: false) Suppress all output. Cannot be used with --debug.

  --debug           (Default: false) Output extra details. Cannot be used with --quiet.

  --delete          (Default: false) Delete old images.

  --help            Display this help screen.

  --version         Display version information.
```

### Examples

If you want to keep a directory in sync with the 10 most recent APOD images to use as rotating wallpapers on your 1080p monitor, you could create a daily cron job with the following:

```
apod-dl -o ~/wallpapers -c 10 -f 1920x1080 --quiet --delete
```

The `-f` option will shrink the image so that it fills a 1920x1080 space (but may overflow it one one of the dimensions depending on the original image's aspect ratio). By specifying `--delete` it ensures that only 10 images will exist in the `wallpapers` directory. Each day when a new one is downloaded, the oldest image will be deleted.
