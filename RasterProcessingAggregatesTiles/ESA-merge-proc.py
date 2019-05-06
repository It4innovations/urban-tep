# ------------------------------------------------------------------------------
# Name:        ESA-merge-proc
# Purpose:
#
# Author:      voj76
#
# Created:     10/07/2018
# Copyright:   (c) voj76 2017
# Licence:     <your licence>
# ------------------------------------------------------------------------------

import argparse
import os
import subprocess
import multiprocessing
import shutil
import logging
import gdal
from time import time

logging.basicConfig(level=logging.DEBUG,
                    format='[%(levelname)s] (%(threadName)-10s) %(message)s',
                    )
cpu_count = multiprocessing.cpu_count()


def gdal_worker(shared_variable):
    """Worker for merging raster files in directory into mosaic."""

    logging.debug('- Started: Mosaic of directory %s' % (shared_variable[0]))
    starting_time = time()

    in_dir = shared_variable[0]
    out_dir = shared_variable[1]
    raster_ext = shared_variable[2]

    mosaic_name = os.path.join(out_dir, os.path.join(os.path.basename(in_dir) + r'_mosaic.' + raster_ext))
    warp_name = os.path.join(out_dir, os.path.join(os.path.basename(in_dir) + r'_warp.' + raster_ext))
    result_name = os.path.join(out_dir, os.path.join(os.path.basename(in_dir) + r'.' + raster_ext))

    os.chdir(in_dir)
    # Generate raster list in current directory
    if os.name == 'nt':
        os.system(r'dir *.' + raster_ext + '/b > raster_files.txt')
    elif os.name == 'posix':
        os.system(r'ls *.' + raster_ext + ' > raster_files.txt')

    # Build vrt list
    return_code = subprocess.call([r'gdalbuildvrt', r'-input_file_list', r'raster_files.txt', r'raster_files.vrt'])
    if return_code == 0:

        # Build mosaic
        return_code = subprocess.call(
            ['gdal_translate', '-a_srs', 'EPSG:4326', '-of', 'GTiff', '-co', 'BigTIFF=NO', '-co', 'COMPRESS=LZW',
             'raster_files.vrt', mosaic_name])
        if return_code == 0:

            data_file = gdal.Open(mosaic_name)
            ts = data_file.RasterXSize
            del data_file

            return_code = subprocess.call(
                ['gdalwarp', '-s_srs', 'EPSG:4326', '-t_srs', 'EPSG:3857', '-ts', str(ts), '0', '-co', 'BigTIFF=YES',
                 mosaic_name, warp_name])
            if return_code == 0:
                os.remove(mosaic_name)

                return_code = subprocess.call(
                    ['gdal_translate', '-a_srs', 'EPSG:3857', '-of', 'GTiff', '-co', 'BigTIFF=NO', '-co',
                     'COMPRESS=LZW', '-co', 'TILED=YES', '-co', 'BLOCKXSIZE=512', '-co', 'BLOCKYSIZE=512', warp_name,
                     result_name])
                if return_code == 0:
                    os.remove(warp_name)

                    # Add overviews
                    return_code = subprocess.call(
                        ['gdaladdo', '-r', 'NEAREST', result_name, '2', '4', '8', '16', '32', '64'])
                    if return_code == 0:
                        os.chdir(path=out_dir)
                        shutil.rmtree(path=in_dir, ignore_errors=True)

    ending_time = round(time() - starting_time, 1)
    logging.debug('- Finished: Mosaic of directory %s' % (shared_variable[0]))
    logging.debug('- Processing time: %s seconds' % (ending_time,))


def main():
    # in_dir = r'D:\U-TEP\Processing-2018-10-25\WSF2015_pr2018_test'
    # out_dir = r'D:\U-TEP\Processing-2018-10-25\WSF2015_pr2018_test-results'
    # raster_ext = r'tif'

    parser = argparse.ArgumentParser()

    parser.add_argument('i', help='Input directory with subdirs.')
    parser.add_argument('o', help='Output directory.')
    parser.add_argument('-ext', help='Raster file extension.', choices=['tif', 'png'], default='tif')
    parser.add_argument('-w', help='Number of workers. Maximum %s.' % (cpu_count - 1), type=int,
                        default=(cpu_count - 1))

    args = parser.parse_args()

    if args.ext == 'tif':
        raster_ext = args.ext
    elif args.ext == 'png':
        raster_ext = args.ext
    else:
        logging.debug('Unsupported raster format %s' % args.ext)

    if args.w <= (cpu_count - 1):
        w = args.w
    else:
        parser.exit(message='Number of workers %s exceeded allowed maximum %s.' % (args.w, cpu_count - 1))

    if args.i:
        in_dir = args.i
        if not os.path.exists(in_dir):
            if os.path.exists(os.path.join(os.getcwd(), in_dir)):
                in_dir = os.path.join(os.getcwd(), in_dir)
            else:
                parser.exit(message='Input %s directory does not exists' % (os.path.join(os.getcwd(), in_dir)))

    if args.o:
        out_dir = args.o
        if not os.path.exists(out_dir):
            os.makedirs(out_dir)

    # Directories to process
    dir_name_list = []
    for (dir_path, dir_names, file_names) in os.walk(in_dir):
        dir_name_list.extend(dir_names)

    shared_variable = []
    for item in dir_name_list:
        shared_variable.append([os.path.join(in_dir, item), out_dir, raster_ext])

    del dir_name_list

    p = multiprocessing.Pool(w)
    p.map(gdal_worker, shared_variable)


if __name__ == '__main__':
    main()
