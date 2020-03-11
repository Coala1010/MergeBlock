//
//  APDNativeAdSettings.h
//  Appodeal
//
//  Created by Stas Kochkin on 04/07/2020.
//  Copyright © 2020 Appodeal, Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Appodeal/APDNativeAdViewProtocol.h>
#import <Appodeal/APDDefines.h>


/**
 Native resource autocache mask
 */
typedef NS_OPTIONS(NSUInteger, APDNativeResourceAutocacheMask) {
    /**
     Autocache icon
     */
    APDNativeResourceAutocacheIcon = 1 << 0,
    /**
     Autocache Media
     */
    APDNativeResourceAutocacheMedia = 1 << 1
};

/**
 Instance of APDNativeAdSettings
 */
@interface APDNativeAdSettings : NSObject
/**
 Set native ad view class
 */
@property (nonatomic, assign, nonnull) Class <APDNativeAdView> adViewClass;
/**
 Set native ad Type
 */
@property (nonatomic, assign) APDNativeAdType type;
/**
 Set autocache mask
 */
@property (nonatomic, assign) APDNativeResourceAutocacheMask autocacheMask;
/**
 Set native placehplder URL
 This url use as pleceholder for mainImage view
 */
@property (nonatomic, strong, nullable) NSURL * nativeMediaViewPlaceholder;
/**
 Default instance
 @return Instance of APDNativeAdSettings
 */
+ (instancetype _Nonnull)defaultSettings;

@end
