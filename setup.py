from setuptools import setup, find_packages
from os import path

here = path.abspath(path.dirname(__file__))

# Get the long description from the README file
with open(path.join(here, 'README.md'), encoding='utf-8') as f:
    long_description = f.read()

setup(
    name='msal_interactive_token_acquirer',
    version='0.1.0',
    description='A library to get MSAL token interactively for native client.',
    long_description=long_description,
    long_description_content_type='text/markdown',
    url='https://github.com/masamitsu-murase/msal_interactive',
    author='Masamitsu MURASE',
    author_email='masamitsu.murase@gmail.com',
    license='MIT',
    keywords='MSAL OAuth2',
    packages=find_packages("src"),
    package_dir={"": "src"},
    include_package_data=True,
    package_data={
        "msal_interactive_token_acquirer": [
            "tools/*.dll",
            "tools/*.exe",
            "tools/*.exe.config"
        ]
    },
    zip_safe=False,
    python_requires='!=3.0.*, !=3.1.*, !=3.2.*, !=3.3.*, !=3.4.*, <4',
    project_urls={
        'Bug Reports':
        'https://github.com/masamitsu-murase/msal_interactive/issues',
        'Source': 'https://github.com/masamitsu-murase/msal_interactive',
    },
)
